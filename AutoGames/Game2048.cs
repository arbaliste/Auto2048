using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;
using Newtonsoft.Json;
using System.IO;
using OpenQA.Selenium.Remote;

namespace AutoGames
{
    public class Game2048 : Game
    {
        const int BoardSize = 4 * 4;
        const int MaxRepeat = 50;

        public Game2048() : base("2048") { }

        protected override Network GenerateNetwork() => new Network(new int[] { BoardSize + 2, 11, 6, 1 }, Network.ActivationFunctions.TanH, Network.MutateFunctions.GenerateReplacement(1));

        protected override void RunTrial(RemoteWebDriver driver, Trial trial)
        {
            driver.FindElementByClassName("restart-button").Click();
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
    }
}
