using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticSnake
{
    class NeuralNetwork
    {
        public List<Layer> Layers;

        public NeuralNetwork(int[] layerNums)
        {
            Layers = new List<Layer>();
            for (int i = 0; i < layerNums.Length; i++)
            {
                Layer l = new Layer(layerNums[i]);
                for (int j = 0; j < layerNums[i]; j++)
                {
                    Node n = new Node();
                    if (i == 0)
                        n.Bias = 0;
                    else
                        n.Edges.AddRange(Enumerable.Range(0, layerNums[i - 1]).Select(x => new Edge()));
                    l.Nodes.Add(n);
                }
                Layers.Add(l);
            }
        }

        private double Activation(double x)
        {
            return (2 / (1 + Math.Exp(-2 * x))) - 1;
        }

        public double[] Run(double[] input)
        {
            if (input.Length != Layers[0].Nodes.Count) return null;

            for (int i = 0; i < Layers.Count; i++)
            {
                for (int j = 0; j < Layers[i].Nodes.Count; j++)
                {
                    Node n = Layers[i].Nodes[j];
                    if (i == 0)
                        n.Value = input[j];
                    else
                        n.Value = Activation(Enumerable.Range(0, Layers[i - 1].Nodes.Count).Select(x => Layers[i - 1].Nodes[x].Value * n.Edges[x].Weight).Sum() + n.Bias);
                }
            }

            return Layers.Last().Nodes.Select(x => x.Value).ToArray();
        }

        public void Mutate(double rate)
        {
            foreach (Layer l in Layers)
            {
                foreach (Node n in l.Nodes)
                {
                    if (Util.RandomSeededDouble() <= rate)
                        n.Bias = Util.RandomSeededInt(-1, 1) + Util.RandomSeededDouble();

                    foreach (Edge e in n.Edges)
                    {
                        if (Util.RandomSeededDouble() <= rate)
                            e.Weight = Util.RandomSeededInt(-1, 1) + Util.RandomSeededDouble();
                    }
                }
            }
        }

        public static NeuralNetwork Cross(NeuralNetwork a, NeuralNetwork b)
        {
            NeuralNetwork child = new NeuralNetwork(a.Layers.Select(x => x.Nodes.Count).ToArray());
            for (int i = 0; i < a.Layers.Count; i++)
            {
                for (int j = 0; j < a.Layers[i].Nodes.Count; j++)
                {
                    child.Layers[i].Nodes[j].Bias = Util.RandomSeededDouble() < 0.5 ? a.Layers[i].Nodes[j].Bias : b.Layers[i].Nodes[j].Bias;
                }
            }
            return child;
        }
    }
}
