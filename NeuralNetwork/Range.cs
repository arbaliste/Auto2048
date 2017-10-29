using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class Range
    {
        public Func<double, double> ActivationFunction;
        public double Min;
        public double Max;

        public Range(Func<double, double> activation, double min, double max)
        {
            ActivationFunction = activation;
            Min = min;
            Max = max;
        }

        public static Range ZeroOne = new Range(x => (1 / (1 + Math.Exp(-1 * x))), 0, 1);
        public static Range NegativeOneOne = new Range(x => ((2 / (1 + Math.Exp(-2 * x))) - 1), -1, 1);
    }
}
