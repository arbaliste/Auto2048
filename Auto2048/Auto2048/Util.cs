using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticSnake
{
    static class Util
    {
        public static double RandomSeededDouble()
        {
            return new Random(Guid.NewGuid().GetHashCode()).NextDouble();
        }

        public static int RandomSeededInt(int min, int max)
        {
            return new Random(Guid.NewGuid().GetHashCode()).Next(min, max);
        }

        public static double Sigmoid(double x)
        {
            return (1 / (1 + Math.Exp(-1 * x)));
        }

        public static double TanH(double x)
        {
            return (2 / (1 + Math.Exp(-2 * x))) - 1;
        }
    }
}
