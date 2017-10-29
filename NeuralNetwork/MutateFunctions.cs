using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public static class MutateFunctions
    {
        public static double Additive(double input, double min, double max) => Math.Max(Math.Min(input + Util.RandomSeededDoubleRange(min - max, max - min) * 0.1, max), min);
        public static double Replacement(double input, double min, double max) => Util.RandomSeededDoubleRange(min, max);
    }
}
