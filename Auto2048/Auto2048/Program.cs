using System;
using System.Linq;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Diagnostics;
using OpenQA.Selenium.Remote;
using Newtonsoft.Json;

namespace AutomaticSnake
{
    class Program
    {
        static void Main(string[] args)
        {
            RemoteWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("http://arbaliste.github.io/Auto2048");

            int size = 4 * 4;
            int maxTrials = 5;
            Trial[] trials = Enumerable.Range(0, maxTrials).Select(x => new Trial()
            {
                Network = new NeuralNetwork(new int[] { size, 9, 1 }, Util.Sigmoid),
                Fitness = 0
            }).ToArray();
            Trial bestTrial = trials[0];

            int gen = 1;

            while (true)
            {
                Console.WriteLine("Running generation " + gen);
                for (int trialNum = 0; trialNum < maxTrials; trialNum++)
                {
                    double[] previous = new double[size];
                    while (true)
                    {
                        // Exits the program when the browser is closed
                        // e has to be here because it will be optimized out?
                        var e = "";
                        try
                        {
                            e = driver.Title;
                        }
                        catch
                        {
                            driver.Dispose();
                            return;
                        }

                        System.Threading.Thread.Sleep(200);
                        var manager = (Dictionary<string, dynamic>)driver.ExecuteScript("return gameManager");
                        bool over = manager["over"];
                        long score = manager["score"];
                        double[] board = ((IEnumerable<object>)(manager["grid"]["cells"])).Cast<IEnumerable<dynamic>>().SelectMany(x => x).Select(x => Math.Log((x?["value"]*2) ?? 2, 2) / 12d).Cast<double>().ToArray();

                        if (over || board.SequenceEqual(previous))
                        {
                            trials[trialNum].Fitness = score;
                            break;
                        }

                        double data = trials[trialNum].Network.Run(board)[0];
                        string sendKeys = "";
                        if (data < 0.25) sendKeys = "w";
                        else if (data < 0.5) sendKeys = "a";
                        else if (data < 0.75) sendKeys = "s";
                        else sendKeys = "d";

                        driver.Keyboard.SendKeys(sendKeys);
                        //System.Threading.Thread.Sleep(100);
                        board.CopyTo(previous, 0);
                    }
                    Console.WriteLine(" - Trial " + trialNum + ": " + trials[trialNum].Fitness);
                    driver.FindElementByClassName("restart-button").Click();
                }
                bestTrial = trials.OrderBy(x => x.Fitness).Last();
                for (int i = 0; i < trials.Length; i++)
                {
                    trials[i].Network = NeuralNetwork.Cross(bestTrial.Network, trials[i].Network);
                    if (trials[i].Network != bestTrial.Network) trials[i].Network.Mutate(0.25);
                }
                Console.WriteLine(" - Best: " + bestTrial.Fitness);
                gen++;
            }
        }
    }
}
