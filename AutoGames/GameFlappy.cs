using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;
using OpenQA.Selenium.Remote;

namespace AutoGames
{
    public class GameFlappy : Game
    {
        public GameFlappy() : base("http://nebezb.com/floppybird/") { }

        protected override Network GenerateNetwork() => new Network(new int[] { 3, 6, 1 }, Network.ActivationFunctions.Sigmoid, Network.MutateFunctions.GenerateReplacement(1));

        protected override void RunTrial(RemoteWebDriver driver, Trial trial)
        {
            driver.Keyboard.SendKeys(" ");
            while (true)
            {
                if (bool.Parse(driver.ExecuteScript("return window.replayclickable").ToString()))
                {
                    trial.Fitness = int.Parse(driver.ExecuteScript("return window.score").ToString());
                    break;
                }

                var playerPos = driver.FindElementById("player").Location;
                var playerSize = driver.FindElementById("player").Size;

                double playerX = playerPos.X + playerSize.Width / 2;
                double playerY = playerPos.Y + playerSize.Height / 2;

                double screenX = driver.FindElementById("flyarea").Size.Width;
                double screenY = driver.FindElementById("flyarea").Size.Height;

                double pipeX = screenX;
                double pipeY = screenY;
                if (driver.ExecuteScript("return window.pipes[0]") != null)
                {
                    // TODO: It's null?
                    pipeX = double.Parse(driver.ExecuteScript("return window.pipes[0].children('.pipe_upper').offset().left - 2").ToString());
                    pipeY = double.Parse(driver.ExecuteScript("return window.pipes[0].children('.pipe_upper').offset().top + window.pipes[0].children('.pipe_upper').height() + pipeheight / 2").ToString());
                }

                double[] inputs = { playerY / screenY, (pipeY - playerY)/screenX, (pipeX - playerX)/screenY };
                double data = trial.Network.Run(inputs)[0];
                if (data > 0.5) driver.Keyboard.SendKeys(" ");
            }
        }
    }
}
