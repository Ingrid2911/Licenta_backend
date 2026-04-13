using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal enum ExpandedEdgeType
    {
        Holdover,   // a_t -> a_{t+1}
        Movement    // a_t -> b_{t+travelTime}
    }

    internal class TimeExpandedGraphBuilder
    {
        public ExpandedGraph BuildTimeExpandedGraph(SimpleGraph simpleGraph, int H)
        {
            HashSet<string> FinalNodes = new HashSet<string>();
            List<ExpandedEdge> FinalEdges = new List<ExpandedEdge>();

            const string SuperSource = "S*";
            const string SuperSink = "T*";
            const int WAIT_CAPACITY = 10; //modificat
            const int INF = 1000;

            // 🔹 1. Creează nodurile pentru fiecare timp
            foreach (string node in simpleGraph.Nodes)
            {
                for (int t = 0; t <= H; t++)
                {
                    string expandedNode = node + "_" + t;
                    FinalNodes.Add(expandedNode);
                }
            }

            // 🔹 2. Muchii de așteptare (holdover)
            foreach (string node in simpleGraph.Nodes)
            {
                for (int t = 0; t < H; t++)
                {
                    string from = node + "_" + t;
                    string to = node + "_" + (t + 1);

                    FinalEdges.Add(new ExpandedEdge(from, to, WAIT_CAPACITY));
                }
            }

            // 🔹 3. Muchii de mișcare
            foreach (SimpleEdge edge in simpleGraph.Edges)
            {
                for (int t = 0; t + edge.TravelTime <= H; t++)
                {
                    string from = edge.From + "_" + t;
                    string to = edge.To + "_" + (t + edge.TravelTime);

                    FinalEdges.Add(new ExpandedEdge(from, to, edge.Capacity));
                }
            }

            // 🔹 4. SuperSource și SuperSink
            FinalNodes.Add(SuperSource);
            FinalNodes.Add(SuperSink);

            string source = simpleGraph.SourceNode;
            string sink = simpleGraph.SinkNode;

            // ✔️ FIX CRITIC: doar la t = 0
            FinalEdges.Add(new ExpandedEdge(SuperSource, source + "_0", INF));

            // ✔️ sink conectat pe toate timpii
            for (int t = 0; t <= H; t++)
            {
                string from = sink + "_" + t;
                FinalEdges.Add(new ExpandedEdge(from, SuperSink, INF));
            }

            // 🔹 5. Creează graful final
            ExpandedGraph finalGraph = new ExpandedGraph(
                FinalNodes,
                FinalEdges,
                SuperSource,
                SuperSink,
                H
            );

            return finalGraph;
        }
    }
}