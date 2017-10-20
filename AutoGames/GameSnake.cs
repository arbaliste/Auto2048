using NeuralNetwork;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGames
{
    public class GameSnake : Game
    {
        public GameSnake() : base("snake") { }

        protected override Network GenerateNetwork() => new Network(new int[] { 6, 8, 1 }, Network.ActivationFunctions.Sigmoid, Network.MutateFunctions.GenerateReplacement(1));

        protected override void RunTrial(RemoteWebDriver driver, Trial trial)
        {
            // TODO: May not work, have not tested
            var container = driver.FindElement(By.Id("sbWelcome0"));
            container.FindElement(By.TagName("button")).Click();

            driver.FindElementsByTagName("button").Last().Click();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int ticks = 0;
            while (true)
            {
                    int xPos = int.Parse(driver.ExecuteScript("return snake.snakeHead.col").ToString());
                    int yPos = int.Parse(driver.ExecuteScript("return snake.snakeHead.row").ToString());
                    List<List<int>> board = ((IReadOnlyCollection<object>)driver.ExecuteScript("return mySnakeBoard.grid")).Select(x => ((IEnumerable<object>)x).Select(y => int.Parse(y.ToString())).ToList()).ToList();
                    List<Dictionary<string, object>> snakeBody = ((IEnumerable<object>)driver.ExecuteScript("return Object.values(snake.snakeBody).map(x => { return { col: x.col, row: x.row }})")).Cast<Dictionary<string, object>>().ToList();
                    int yFoodPos = board.FindIndex(x => x.Contains(-1));
                    int xFoodPos = board[yFoodPos].FindIndex(x => x == -1);

                    double diffX = (xFoodPos - xPos) / (double)board[0].Count;
                    double diffY = (yFoodPos - yPos) / (double)board.Count;

                    for (int i = 1; i < snakeBody.Count; i++)
                    {
                        board[(int)snakeBody[i]["col"]][(int)snakeBody[i]["row"]] = 1;
                    }

                    // Do not reward inactivity
                    if (stopwatch.ElapsedMilliseconds > 1000 && int.Parse(driver.ExecuteScript("return mySnakeBoard.getBoardState()").ToString()) == 1)
                    {
                        trial.Fitness = -1;
                        break;
                    }

                    // Dead
                    if (stopwatch.ElapsedMilliseconds > 1000 * 100 || xPos == 0 || yPos == 0 || xPos == board[0].Count || yPos == board.Count || int.Parse(driver.ExecuteScript("return mySnakeBoard.getBoardState()").ToString()) == 0)
                    {
                        trial.Fitness = ticks + 1000 * int.Parse(driver.ExecuteScript("return snake.snakeLength").ToString());
                        break;
                    }


                    double top = board[yPos - 1][xPos] == 1 ? 1 : -1;
                    double bottom = board[yPos + 1][xPos] == 1 ? 1 : -1;
                    double left = board[yPos][xPos - 1] == 1 ? 1 : -1;
                    double right = board[yPos][xPos + 1] == 1 ? 1 : -1;

                    double data = trial.Network.Run(new double[] { diffX, diffY, top, bottom, left, right })[0];
                    string sendKeys = "";
                    if (data < 0.2) sendKeys = "w";
                    else if (data < 0.4) sendKeys = "a";
                    else if (data < 0.6) sendKeys = "s";
                    else if (data < 0.8) sendKeys = "d";
                    else sendKeys = "";

                    driver.Keyboard.SendKeys(sendKeys);
                    System.Threading.Thread.Sleep(100);
                    ticks++;
            }
        }
    }
}
