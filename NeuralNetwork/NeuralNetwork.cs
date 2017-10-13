using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class NeuralNetwork
    {
        public List<Layer> Layers;
        public Func<double, double> ActivationFunction;
        public Func<double, double> MutationFunction;

        public NeuralNetwork(int[] layerNums, Func<double, double> activation, Func<double, double> mutation)
        {
            ActivationFunction = activation;
            MutationFunction = mutation;
            Layers = new List<Layer>();
            for (int i = 0; i < layerNums.Length; i++)
            {
                Layer layer = new Layer(layerNums[i]);
                for (int j = 0; j < layerNums[i]; j++)
                {
                    Node n = new Node();
                    if (i == 0)
                        n.Bias = 0;
                    else
                        n.Edges.AddRange(Enumerable.Range(0, layerNums[i - 1]).Select(x => new Edge()));
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
                        n.Value = ActivationFunction(Layers[i - 1].Nodes.Select((x, index) => x.Value * n.Edges[index].Weight).Sum() + n.Bias);
                }
            }

            return Layers.Last().Nodes.Select(x => x.Value).ToArray();
        }

        public static class ActivationFunctions
        {
            public static double Sigmoid(double x)
            {
                return (1 / (1 + Math.Exp(-1 * x)));
            }

            public static double TanH(double x)
            {
                return (2 / (1 + Math.Exp(-2 * x))) - 1;
            }
        }

        public static class MutateFunctions
        {
            public static Func<double, double> GenerateAdditive(double rate)
            {
                Func<double, double> replacement = GenerateReplacement(rate);
                return (input) =>
                {
                    return input + replacement(input);
                };
            }

            public static Func<double, double> GenerateReplacement(double rate)
            {
                return (input) =>
                {
                    return (Util.RandomSeededDouble() * 2 - 1) * rate;
                };
            }
        }

        public void Mutate(double rate)
        {
            foreach (Layer l in Layers)
            {
                foreach (Node n in l.Nodes)
                {
                    if (Util.RandomSeededDouble() <= rate)
                        n.Bias = MutationFunction(n.Bias);

                    foreach (Edge e in n.Edges)
                    {
                        if (Util.RandomSeededDouble() <= rate)
                            e.Weight = MutationFunction(e.Weight);
                    }
                }
            }
        }

        public static NeuralNetwork Cross(NeuralNetwork a, NeuralNetwork b)
        {
            NeuralNetwork child = new NeuralNetwork(a.Layers.Select(x => x.Nodes.Count).ToArray(), a.ActivationFunction, a.MutationFunction);
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
