using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph
{
    internal class GraphGenerator
    {
        public static SimpleGraph GenerateGraph(int totalNodes)
        {
            if (totalNodes < 4)
                throw new ArgumentException("Trebuie minim 4 noduri (s + t + altele)");

            var nodes = new List<string>();
            var edges = new List<SimpleEdge>();

            nodes.Add("s");
            nodes.Add("t");

            int intermediateNodes = totalNodes - 2;

            // 🔹 determinăm nivelurile automat (aprox sqrt)
            int levels = (int)Math.Sqrt(intermediateNodes);
            int nodesPerLevel = intermediateNodes / levels;

            // dacă nu se împarte perfect
            while (levels * nodesPerLevel < intermediateNodes)
                nodesPerLevel++;

            // 🔹 generare noduri
            for (int l = 1; l <= levels; l++)
            {
                for (int i = 1; i <= nodesPerLevel; i++)
                {
                    nodes.Add($"L{l}_{i}");
                }
            }

            Random rnd = new Random(42);

            // 🔹 s → primul nivel
            for (int i = 1; i <= nodesPerLevel; i++)
            {
                edges.Add(new SimpleEdge("s", $"L1_{i}", 1, rnd.Next(3, 7)));
            }

            // 🔹 conexiuni între niveluri
            for (int l = 1; l < levels; l++)
            {
                for (int i = 1; i <= nodesPerLevel; i++)
                {
                    for (int j = 1; j <= nodesPerLevel; j++)
                    {
                        // nu conectăm chiar tot (control densitate)
                        if (rnd.NextDouble() < 0.6)
                        {
                            edges.Add(new SimpleEdge(
                                $"L{l}_{i}",
                                $"L{l + 1}_{j}",
                                1,
                                rnd.Next(1, 5)
                            ));
                        }
                    }
                }
            }

            // 🔹 ultim nivel → t
            for (int i = 1; i <= nodesPerLevel; i++)
            {
                edges.Add(new SimpleEdge($"L{levels}_{i}", "t", 1, rnd.Next(3, 8)));
            }

            return new SimpleGraph(nodes, edges, "s", "t");
        }
    }
}
