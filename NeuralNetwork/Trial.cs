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
        public Network Network;
        public double Fitness;

        public static class BreedMethods
        {
            public static void Aggressive(Trial[] trials, double rate)
            {
                Trial bestTrial = trials.OrderBy(x => x.Fitness).Last();
                Trial[] worstTrials = trials.OrderBy(x => x.Fitness).Take(trials.Length / 2).ToArray();
                for (int i = 0; i < trials.Length; i++)
                {
                    Trial trial = trials[i];
                    if (worstTrials.Contains(trial))
                        trial.Network = Network.Cross(bestTrial.Network, bestTrial.Network);
                    else
                        trial.Network = Network.Cross(bestTrial.Network, trial.Network);
                    if (trial != bestTrial) trial.Network.Mutate(rate);
                }
            }

            public static void Passive(Trial[] trials, double rate)
            {
                Trial bestTrial = trials.OrderBy(x => x.Fitness).Last();
                for (int i = 0; i < trials.Length; i++)
                {
                    Trial trial = trials[i];
                    trial.Network = Network.Cross(bestTrial.Network, trial.Network);
                    if (trial != bestTrial) trial.Network.Mutate(rate);
                }
            }
        }
    }
}
