using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class Trial
    {
        public NeuralNetwork Network;
        public double Fitness;
        public RemoteWebDriver Driver;

        public static class BreedMethods
        {
            public static void Aggressive(Trial[] trials, double rate)
            {
                Trial bestTrial = trials.OrderBy(x => x.Fitness).Last();
                Trial[] worstTrials = trials.OrderBy(x => x.Fitness).Take(trials.Length / 2).ToArray();
                for (int i = 0; i < trials.Length; i++)
                {
                    if (worstTrials.Contains(trials[i]))
                        trials[i].Network = NeuralNetwork.Cross(bestTrial.Network, bestTrial.Network);
                    else
                        trials[i].Network = NeuralNetwork.Cross(bestTrial.Network, trials[i].Network);
                    if (trials[i] != bestTrial) trials[i].Network.Mutate(rate);
                }
            }

            public static void Passive(Trial[] trials, double rate)
            {
                Trial bestTrial = trials.OrderBy(x => x.Fitness).Last();
                for (int i = 0; i < trials.Length; i++)
                {
                    trials[i].Network = NeuralNetwork.Cross(bestTrial.Network, trials[i].Network);
                    if (trials[i] != bestTrial) trials[i].Network.Mutate(rate);
                }
            }
        }
    }
}
