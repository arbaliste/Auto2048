using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class Layer
    {
        public List<Node> Nodes;
        
        public Layer(int count)
        {
            Nodes = new List<Node>(count);
        }
    }
}
