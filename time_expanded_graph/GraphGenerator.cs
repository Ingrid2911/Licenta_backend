using System;
using System.Collections.Generic;
using System.Linq;

namespace time_expanded_graph
{
    internal class GraphGenerator
    {
        public static SimpleGraph GenerateGraph(
            int totalNodes,
            int minCapacity,
            int maxCapacity)
        {
            if (totalNodes < 4)
                throw new ArgumentException("Minim 4 noduri");

            int intermediateNodes = totalNodes - 2;

            var nodes = new List<string>();
            var edges = new List<SimpleEdge>();

            nodes.Add("s");
            nodes.Add("t");

            List<string> alphabetNodes = new();

            for (int i = 0; i < intermediateNodes; i++)
            {
                alphabetNodes.Add(((char)('a' + i)).ToString());
            }

            nodes.AddRange(alphabetNodes);

            Random rnd = new();

            int levels = Math.Max(2, (int)Math.Sqrt(intermediateNodes));
            int nodesPerLevel = (int)Math.Ceiling((double)intermediateNodes / levels);

            List<List<string>> levelNodes = new();

            int index = 0;
            for (int l = 0; l < levels; l++)
            {
                var level = new List<string>();

                for (int i = 0; i < nodesPerLevel && index < alphabetNodes.Count; i++)
                {
                    level.Add(alphabetNodes[index]);
                    index++;
                }

                if (level.Count > 0)
                    levelNodes.Add(level);
            }

            foreach (var node in levelNodes[0])
            {
                edges.Add(new SimpleEdge(
                    "s",
                    node,
                    1,
                    rnd.Next(minCapacity, maxCapacity + 1)
                ));
            }

            for (int l = 0; l < levelNodes.Count - 1; l++)
            {
                foreach (var from in levelNodes[l])
                {
                    foreach (var to in levelNodes[l + 1])
                    {
                        if (rnd.NextDouble() < 0.7)
                        {
                            edges.Add(new SimpleEdge(
                                from,
                                to,
                                1,
                                rnd.Next(minCapacity, maxCapacity + 1)
                            ));
                        }
                    }
                }
            }

            foreach (var node in levelNodes.Last())
            {
                edges.Add(new SimpleEdge(
                    node,
                    "t",
                    1,
                    rnd.Next(minCapacity, maxCapacity + 1)
                ));
            }

            return new SimpleGraph(nodes, edges, "s", "t");
        }
    }
}