using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class Network
    {
        public List<Layer> Layers;
        public Range Range;
        public Func<double, double, double, double> MutationFunction;

        public Network(int[] layerNums, Range range, Func<double, double, double, double> mutation)
        {
            Range = range;
            MutationFunction = mutation;
            Layers = new List<Layer>();
            for (int i = 0; i < layerNums.Length; i++)
            {
                Layer layer = new Layer(layerNums[i]);
                for (int j = 0; j < layerNums[i]; j++)
                {
                    Node n = new Node() { Bias = Util.RandomSeededDoubleRange(Range.Min, Range.Max) };
                    if (i != 0)
                        n.Edges.AddRange(Enumerable.Range(0, layerNums[i - 1]).Select(x => new Edge() { Weight = Util.RandomSeededDoubleRange(Range.Min, Range.Max) }));
                    layer.Nodes.Add(n);
                }
                Layers.Add(layer);
            }
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
                        n.Value = Range.ActivationFunction(Layers[i - 1].Nodes.Select((x, index) => x.Value * n.Edges[index].Weight).Sum() + n.Bias);
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
                        n.Bias = MutationFunction(n.Bias, Range.Min, Range.Max);

                    foreach (Edge e in n.Edges)
                    {
                        if (Util.RandomSeededDouble() <= rate)
                            e.Weight = MutationFunction(e.Weight, Range.Min, Range.Max);
                    }
                }
            }
        }

        public static Network Cross(Network a, Network b)
        {
            Network child = new Network(a.Layers.Select(x => x.Nodes.Count).ToArray(), a.Range, a.MutationFunction);
            for (int i = 0; i < a.Layers.Count; i++)
            {
                for (int j = 0; j < a.Layers[i].Nodes.Count; j++)
                {
                    Node childNode = child.Layers[i].Nodes[j];
                    Node copy = (Util.RandomSeededDouble() < 0.5 ? a : b).Layers[i].Nodes[j];
                    for (int e = 0; e < child.Layers[i].Nodes[j].Edges.Count; e++)
                    {
                        childNode.Edges[e].Weight = copy.Edges[e].Weight;
                    }
                    childNode.Bias = copy.Bias;
                }
            }
            return child;
        }

    }
}
