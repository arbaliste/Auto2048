using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticSnake
{
    class Edge
    {
        public double Weight;

        public Edge()
        {
            Weight = Util.RandomSeededDouble();
        }
    }
}
