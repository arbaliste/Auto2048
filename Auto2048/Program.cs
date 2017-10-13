using System;
using System.Linq;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Newtonsoft.Json;
using System.Threading.Tasks;
using NeuralNetwork;

namespace NeuralNetwork
{
    class Program
    {
        const int BoardSize = 4 * 4;
        const int MaxDrivers = 10;
        const int TrialsPerDriver = 4;
        const int MaxTrials = MaxDrivers * TrialsPerDriver;

        static void Main(string[] args)
        {
            int generation = 1;

            ChromeDriver[] drivers = Enumerable.Range(0, MaxDrivers).Select(x => new ChromeDriver()).ToArray();
            Trial[] trials = Enumerable.Range(0, MaxTrials).Select(x => new Trial()
            {
                Network = new NeuralNetwork(new int[] { BoardSize, 9, 1 }, NeuralNetwork.ActivationFunctions.TanH, NeuralNetwork.MutateFunctions.GenerateReplacement(1)),
                Fitness = 0,
            }).ToArray();

            foreach (var driver in drivers)
                driver.Navigate().GoToUrl("http://arbaliste.github.io/Auto2048");


            while (true)
            {
                Console.WriteLine("Running generation " + generation);
                Parallel.ForEach(drivers, (driver, _, driverNum) =>
                {
                    for (int trialNum = 0; trialNum < TrialsPerDriver; trialNum++)
                    {
                        Trial trial = trials[driverNum * TrialsPerDriver + trialNum];
                        driver.FindElementByClassName("restart-button").Click();
                        System.Threading.Thread.Sleep(200);
                        double[] previous = new double[BoardSize];
                        while (true)
                        {
                            var manager = (Dictionary<string, dynamic>)driver.ExecuteScript("return gameManager");
                            bool over = manager["over"];
                            long score = manager["score"];
                            double[] board = ((IEnumerable<object>)(manager["grid"]["cells"])).Cast<IEnumerable<dynamic>>().SelectMany(x => x).Select(x => Math.Log((x?["value"] * 2) ?? 2, 2) / 12d).Cast<double>().ToArray();

                            if (over || board.SequenceEqual(previous))
                            {
                                trial.Fitness = score;
                                break;
                            }

                            double data = trial.Network.Run(board)[0];
                            string sendKeys = "";
                            if (data < -0.5) sendKeys = "w";
                            else if (data < 0) sendKeys = "a";
                            else if (data < 0.5) sendKeys = "s";
                            else sendKeys = "d";

                            driver.Keyboard.SendKeys(sendKeys);
                            board.CopyTo(previous, 0);
                        }
                    }
                });
                for (int i = 0; i < trials.Length; i++)
                    Console.WriteLine(" - Trial " + i + ": " + trials[i].Fitness);
                Trial.BreedMethods.Aggressive(trials, 0.3);
                Console.WriteLine(" - Best: " + trials.OrderBy(x => x.Fitness).Last().Fitness);
                generation++;
            }
        }
    }
}
