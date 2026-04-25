using System;
using System.Collections.Generic;
using System.Linq;

namespace time_expanded_graph
{
    internal class GraphGenerator
    {
        public static SimpleGraph GenerateGraph(int totalNodes)
        {
            if (totalNodes < 4)
                throw new ArgumentException("Minim 4 noduri (s + t + altele)");

            int intermediateNodes = totalNodes - 2;

            if (intermediateNodes > 26)
                throw new ArgumentException("Max 26 noduri intermediare (alfabet)");

            var nodes = new List<string>();
            var edges = new List<SimpleEdge>();

            // 🔹 s și t
            nodes.Add("s");
            nodes.Add("t");

            // 🔹 generare noduri alfabet (a, b, c...)
            List<string> alphabetNodes = new List<string>();

            for (int i = 0; i < intermediateNodes; i++)
            {
                char letter = (char)('a' + i);
                alphabetNodes.Add(letter.ToString());
            }

            nodes.AddRange(alphabetNodes);

            Random rnd = new Random(42);

            // 🔹 determinăm niveluri (pentru structură)
            int levels = (int)Math.Sqrt(intermediateNodes);
            int nodesPerLevel = intermediateNodes / levels;

            while (levels * nodesPerLevel < intermediateNodes)
                nodesPerLevel++;

            // 🔹 împărțim nodurile pe niveluri
            List<List<string>> levelNodes = new List<List<string>>();

            int index = 0;
            for (int l = 0; l < levels; l++)
            {
                var level = new List<string>();

                for (int i = 0; i < nodesPerLevel && index < alphabetNodes.Count; i++)
                {
                    level.Add(alphabetNodes[index]);
                    index++;
                }

                levelNodes.Add(level);
            }

            // 🔹 s → primul nivel
            foreach (var node in levelNodes[0])
            {
                edges.Add(new SimpleEdge("s", node, 1, rnd.Next(20, 40)));
            }

            // 🔹 conexiuni între niveluri (mai rare → mai lizibil)
            for (int l = 0; l < levelNodes.Count - 1; l++)
            {
                foreach (var from in levelNodes[l])
                {
                    foreach (var to in levelNodes[l + 1])
                    {
                        if (rnd.NextDouble() < 0.5) // 🔥 densitate mai mică
                        {
                            edges.Add(new SimpleEdge(
                                from,
                                to,
                                1,
                                rnd.Next(15, 35)
                            ));
                        }
                    }
                }
            }

            // 🔹 ultim nivel → t
            foreach (var node in levelNodes.Last())
            {
                edges.Add(new SimpleEdge(node, "t", 1, rnd.Next(20, 40)));
            }

            return new SimpleGraph(nodes, edges, "s", "t");
        }
    }
}