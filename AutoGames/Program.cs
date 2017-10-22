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

namespace AutoGames
{
    class Program
    {
        public static void Main(string[] args)
        {
            new GameSnake().Run();
        }
    }
}
