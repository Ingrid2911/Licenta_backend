using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal enum ExpandedEdgeType //enum pentru tipul de muchie
    {
        Holdover,   // a_2 -> a_3
        Movement    // a_2 -> b_3
    }

    internal class TimeExpandedGraphBuilder
    {
        public ExpandedGraph BuildTimeExpandedGraph(SimpleGraph simpleGraph, int H) { 
            HashSet<string> FinalNodes = new HashSet<string>();
            List<ExpandedEdge> FinalEdges = new List<ExpandedEdge>();
            const string SuperSource = "S*";
            const string SuperSink = "T*";
            const int WAIT_CAPACITY = 1000;

            foreach (string node in simpleGraph.Nodes)//am facut nodurile pentru fiecare moment de timp si muchiile de asteptare
            {
                for(int t=1;t<=H;t++)
                {
                    string expandedNode=node+"_"+t;
                    FinalNodes.Add(expandedNode);
                    //string from = node+"_"+t;
                    string to= node+"_"+(t+1);
                    FinalEdges.Add(new ExpandedEdge(expandedNode, to, WAIT_CAPACITY));
                }
            }
            foreach(SimpleEdge edge in simpleGraph.Edges) //muchiile de miscare
            {
                for (int t = 1; t + edge.TravelTime <= H; t++)
                {
                    string from = edge.From + "_" + t;
                    string to = edge.To + "_" + (t + edge.TravelTime);
                    FinalEdges.Add(new ExpandedEdge(from, to, edge.Capacity));
                }

            }
            FinalNodes.Add(SuperSource);
            FinalNodes.Add(SuperSink);

            string source = simpleGraph.SourceNode; //SuperSursa
            const int INF = 1000;

            for (int t = 1; t <= H; t++)
            {
                string to = source + "_" + t;

                FinalEdges.Add(
                    new ExpandedEdge(
                        SuperSource,
                        to,
                        INF)
                );
            }

            string sink = simpleGraph.SinkNode;//SuperDestinatia

            for (int t = 1; t <= H; t++)
            {
                string from = sink + "_" + t;

                FinalEdges.Add(
                    new ExpandedEdge(
                        from,
                        SuperSink,
                        INF)
                );
            }

            ExpandedGraph finalGraph = new ExpandedGraph(FinalNodes, FinalEdges, SuperSource,SuperSink,H);
            
            return finalGraph;
        }
    }
}
