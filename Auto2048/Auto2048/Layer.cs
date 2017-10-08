using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticSnake
{
    class Layer
    {
        public List<Node> Nodes;
        
        public Layer(int count)
        {
            Nodes = new List<Node>(count);
        }
    }
}
