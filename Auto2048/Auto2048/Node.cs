using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticSnake
{
    class Node
    {
        public List<Edge> Edges;
        public double Bias;
        public double Value;

        public Node()
        {
            Edges = new List<Edge>();
            Bias = Util.RandomSeededDouble();
        }
    }
}
