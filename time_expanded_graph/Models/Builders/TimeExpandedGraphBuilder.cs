using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.Models.Builders
{
    internal class TimeExpandedGraphBuilder
    {
        public ExpandedGraph BuildTimeExpandedGraph(SimpleGraph simpleGraph, int H)
        {
            HashSet<string> FinalNodes = new HashSet<string>();
            List<ExpandedEdge> FinalEdges = new List<ExpandedEdge>();

            const string SuperSource = "S*";
            const string SuperSink = "T*";
            const int WAIT_CAPACITY = 10; 
            const int INF = 1000;

            foreach (string node in simpleGraph.Nodes)
            {
                for (int t = 0; t <= H; t++)
                {
                    string expandedNode = node + "_" + t;
                    FinalNodes.Add(expandedNode);
                }
            }

            foreach (string node in simpleGraph.Nodes)
            {
                for (int t = 0; t < H; t++)
                {
                    string from = node + "_" + t;
                    string to = node + "_" + (t + 1);

                    FinalEdges.Add(new ExpandedEdge(from, to, WAIT_CAPACITY, ExpandedEdgeType.Holdover));
                }
            }

            foreach (SimpleEdge edge in simpleGraph.Edges)
            {
                for (int t = 0; t + edge.TravelTime <= H; t++)
                {
                    string from = edge.From + "_" + t;
                    string to = edge.To + "_" + (t + edge.TravelTime);

                    FinalEdges.Add(new ExpandedEdge(from, to, edge.Capacity, ExpandedEdgeType.Movement));
                }
            }

            FinalNodes.Add(SuperSource);
            FinalNodes.Add(SuperSink);

            string source = simpleGraph.SourceNode;
            string sink = simpleGraph.SinkNode;

            FinalEdges.Add(new ExpandedEdge(SuperSource, source + "_0", INF, ExpandedEdgeType.Movement));

            for (int t = 0; t <= H; t++)
            {
                string from = sink + "_" + t;
                FinalEdges.Add(new ExpandedEdge(from, SuperSink, INF, ExpandedEdgeType.Movement));
            }

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