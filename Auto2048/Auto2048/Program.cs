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
        static void Main(string[] args)
        {

            int size = 4 * 4;
            int maxTrials = 20;
            Trial[] trials = Enumerable.Range(0, maxTrials).Select(x => new Trial()
            {
                Network = new NeuralNetwork(new int[] { size, 9, 1 }, NeuralNetwork.ActivationFunctions.TanH),
                Fitness = 0,
                Driver = new ChromeDriver()
            }).ToArray();

            foreach (Trial t in trials)
                t.Driver.Navigate().GoToUrl("http://arbaliste.github.io/Auto2048");

            int gen = 1;

            while (true)
            {
                Console.WriteLine("Running generation " + gen);
                Parallel.ForEach(trials, (trial) =>
                {
                    trial.Driver.FindElementByClassName("restart-button").Click();
                    System.Threading.Thread.Sleep(200);
                    double[] previous = new double[size];
                    while (true)
                    {
                        var manager = (Dictionary<string, dynamic>)trial.Driver.ExecuteScript("return gameManager");
                        bool over = manager["over"];
                        long score = manager["score"];
                        double[] board = ((IEnumerable<object>)(manager["grid"]["cells"])).Cast<IEnumerable<dynamic>>().SelectMany(x => x).Select(x => Math.Log(x?["value"] ?? -1, 2) / 11d).Cast<double>().ToArray();

                        if (over || board.SequenceEqual(previous))
                        {
                            trial.Fitness = score;
                            break;
                        }

                        double data = trial.Network.Run(board)[0];
                        string sendKeys = "";
                        if (data < 0.25) sendKeys = "w";
                        else if (data < 0.5) sendKeys = "a";
                        else if (data < 0.75) sendKeys = "s";
                        else sendKeys = "d";

                        trial.Driver.Keyboard.SendKeys(sendKeys);
                        //System.Threading.Thread.Sleep(100);
                        board.CopyTo(previous, 0);
                    }
                });
                for (int i = 0; i < trials.Length; i++)
                    Console.WriteLine(" - Trial " + i + ": " + trials[i].Fitness);
                Trial.BreedMethods.Aggressive(trials, 0.3);
                Console.WriteLine(" - Best: " + trials.OrderBy(x => x.Fitness).Last().Fitness);
                gen++;
            }
        }
    }
}
