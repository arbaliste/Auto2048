using NeuralNetwork;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGames
{
    public abstract class Game
    {
        const int MaxDrivers = 20;
        const int TrialsPerDriver = 10;
        const int MaxTrials = MaxDrivers * TrialsPerDriver;
        const string backupFile = "network.json";

        public string Url;

        public Game(string url)
        {
            Url = url;
        }

        protected abstract Network GenerateNetwork();
        protected abstract void RunTrial(RemoteWebDriver driver, Trial trial);

        public void Run()
        {
            int generation = 1;

            ChromeDriver[] drivers = Enumerable.Range(0, MaxDrivers).Select(x => new ChromeDriver()).ToArray();
            Trial[] trials = Enumerable.Range(0, MaxTrials).Select(x => new Trial()
            {
                Network = GenerateNetwork(),
                Fitness = 0
            }).ToArray();

            if (File.Exists(backupFile))
            {
                Console.WriteLine("Generating networks from backup");
                var layers = JsonConvert.DeserializeObject<List<List<Layer>>>(File.ReadAllText(backupFile));
                for (int i = 0; i < trials.Length; i++)
                {
                    trials[i].Network.Layers = layers[i];
                }
            }
            else
            {
                Console.WriteLine("Generating networks randomly");
            }

            foreach (var driver in drivers)
                driver.Navigate().GoToUrl(Url);


            while (true)
            {
                Parallel.ForEach(drivers, (driver, _, driverNum) =>
                {
                    for (int trialNum = 0; trialNum < TrialsPerDriver; trialNum++)
                        RunTrial(driver, trials[driverNum * TrialsPerDriver + trialNum]);
                });
                Trial.BreedMethods.Aggressive(trials, 0.3);
                Console.WriteLine($"Gen {generation} | Max: {trials.Max(x => x.Fitness)} | Avg: {trials.Average(x => x.Fitness)} | Top: {String.Join(", ", trials.OrderByDescending(x => x.Fitness).Take(5).Select(x => x.Fitness))}");
                File.WriteAllText(backupFile, JsonConvert.SerializeObject(trials.Select(x => x.Network.Layers)));
                generation++;
            }
        }
    }
}
