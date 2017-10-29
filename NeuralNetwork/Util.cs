using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public static class Util
    {
        private static Random GuidRandom() => new Random(Guid.NewGuid().GetHashCode());

        public static double RandomSeededDouble()
        {
            return GuidRandom().NextDouble();
        }

        public static int RandomSeededInt(int min, int max)
        {
            return GuidRandom().Next(min, max);
        }

        public static double RandomSeededDoubleRange(double min, double max)
        {
            return RandomSeededDouble() * (max - min) + min;
        }
    }
}
