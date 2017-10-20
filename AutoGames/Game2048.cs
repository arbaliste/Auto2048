using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;
using Newtonsoft.Json;
using System.IO;

namespace AutoGames
{
    public class Game2048 : Game
    {
        const int BoardSize = 4 * 4;
        const int MaxDrivers = 10;
        const int TrialsPerDriver = 10;
        const int MaxTrials = MaxDrivers * TrialsPerDriver;
        const int MaxRepeat = 50;
        const string backupFile = "network.json";

        public override void Run()
        {
            int generation = 1;

            ChromeDriver[] drivers = Enumerable.Range(0, MaxDrivers).Select(x => new ChromeDriver()).ToArray();
            Trial[] trials = Enumerable.Range(0, MaxTrials).Select(x => new Trial()
            {
                Network = new Network(new int[] { BoardSize + 2, 11, 6, 1 }, Network.ActivationFunctions.TanH, Network.MutateFunctions.GenerateReplacement(1)),
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
                driver.Navigate().GoToUrl("http://arbaliste.github.io/AutoGames/2048");


            while (true)
            {
                Parallel.ForEach(drivers, (driver, _, driverNum) =>
                {
                    for (int trialNum = 0; trialNum < TrialsPerDriver; trialNum++)
                    {
                        Trial trial = trials[driverNum * TrialsPerDriver + trialNum];
                        driver.FindElementByClassName("restart-button").Click();
                        //System.Threading.Thread.Sleep(200);
                        double[] previous = new double[BoardSize];
                        double previousMove = 0;
                        int repeatNum = 0;
                        while (true)
                        {
                            var manager = (Dictionary<string, dynamic>)driver.ExecuteScript("return gameManager");
                            bool over = manager["over"];
                            long score = manager["score"];
                            double[] board = ((IEnumerable<object>)(manager["grid"]["cells"])).Cast<IEnumerable<dynamic>>().SelectMany(x => x).Select(x => Math.Log((x?["value"] * 2) ?? 2, 2) / 12d).Cast<double>().ToArray();
                            List<double> inputs = new List<double>();
                            inputs.AddRange(board);
                            inputs.Add(board.SequenceEqual(previous) ? 1 : -1);
                            inputs.Add(previousMove);

                            if (board.SequenceEqual(previous))
                                repeatNum++;
                            else
                                repeatNum = 0;

                            if (over || repeatNum > MaxRepeat)
                            {
                                trial.Fitness = score;
                                break;
                            }

                            double data = trial.Network.Run(inputs.ToArray())[0];
                            string sendKeys = "";
                            if (data < -0.5) sendKeys = "w";
                            else if (data < 0) sendKeys = "a";
                            else if (data < 0.5) sendKeys = "s";
                            else sendKeys = "d";

                            previousMove = data;

                            driver.Keyboard.SendKeys(sendKeys);
                            board.CopyTo(previous, 0);

                        }
                    }
                });
                Trial.BreedMethods.Aggressive(trials, 0.3);
                Console.WriteLine($"Gen {generation} | Max: {trials.Max(x => x.Fitness)} | Avg: {trials.Average(x => x.Fitness)} | Top: {String.Join(", ", trials.OrderByDescending(x => x.Fitness).Take(5).Select(x => x.Fitness))}");
                File.WriteAllText(backupFile, JsonConvert.SerializeObject(trials.Select(x => x.Network.Layers)));
                generation++;
            }
        }
    }
}
