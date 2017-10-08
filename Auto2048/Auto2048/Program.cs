using System;
using System.Linq;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutomaticSnake
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromeDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("http://arbaliste.github.io/Auto2048");
            var container = driver.FindElement(By.Id("sbWelcome0"));
            container.FindElement(By.TagName("button")).Click();

            int maxTrials = 5;
            Trial[] trials = Enumerable.Range(0, maxTrials).Select(x => new Trial()
            {
                Network = new NeuralNetwork(new int[] { 6, 8, 1 }),
                Fitness = 0
            }).ToArray();
            Trial bestTrial = trials[0];

            Stopwatch stopwatch = new Stopwatch();

            int gen = 1;

            while (true)
            {
                Console.WriteLine("Running generation " + gen);
                for (int trialNum = 0; trialNum < maxTrials; trialNum++)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    int ticks = 0;
                    while (true)
                    {
                        try
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
                                trials[trialNum].Fitness = -1;
                                break;
                            }

                            // Dead
                            if (stopwatch.ElapsedMilliseconds > 1000 * 100 || xPos == 0 || yPos == 0 || xPos == board[0].Count || yPos == board.Count || int.Parse(driver.ExecuteScript("return mySnakeBoard.getBoardState()").ToString()) == 0)
                            {
                                trials[trialNum].Fitness = ticks + 1000 * int.Parse(driver.ExecuteScript("return snake.snakeLength").ToString());
                                break;
                            }


                            double top = board[yPos - 1][xPos] == 1 ? 1 : -1;
                            double bottom = board[yPos + 1][xPos] == 1 ? 1 : -1;
                            double left = board[yPos][xPos - 1] == 1 ? 1 : -1;
                            double right = board[yPos][xPos + 1] == 1 ? 1 : -1;

                            string keys = "wasd";
                            double data = trials[trialNum].Network.Run(new double[] { diffX, diffY, top, bottom, left, right })[0];
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
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            // TODO: Change
                            trials[trialNum].Fitness = -1;
                            break;
                        }
                    }
                    try
                    {
                        driver.FindElementsByTagName("button").Last().Click();
                    }
                    catch { }
                    Console.WriteLine(" - Trial " + trialNum + ": " + trials[trialNum].Fitness);
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
