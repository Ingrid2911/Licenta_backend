using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using time_expanded_graph.ExpandedTimeGraph;
using time_expanded_graph.MaxFlowAlgorithms;

namespace time_expanded_graph.Edmonds_Karp
{
    internal class EdmondsKarpSolver
    {
        private List<List<FlowEdge>> graph;
        private Dictionary<string, int> nodeIndex;

        public EdmondsKarpSolver(ExpandedGraph eg)
        {
            BuildGraph(eg);
        }

        public int ComputeMaxFlow(string source, string sink)
        {
            int s = nodeIndex[source];
            int t = nodeIndex[sink];
            return EdmondsKarp(s, t);
        }

        private void BuildGraph(ExpandedGraph eg)
        {
            nodeIndex = new Dictionary<string, int>();
            graph = new List<List<FlowEdge>>();

            int index = 0;

            // 🔥 colectează TOATE nodurile din muchii
            var allNodes = new HashSet<string>();

            foreach (var edge in eg.ExpandedEdges)
            {
                allNodes.Add(edge.From);
                allNodes.Add(edge.To);
            }

            // adaugă și super source / sink (siguranță)
            allNodes.Add(eg.SuperSource);
            allNodes.Add(eg.SuperSink);

            // mapare
            foreach (var node in allNodes)
            {
                nodeIndex[node] = index++;
                graph.Add(new List<FlowEdge>());
            }

            // adaugă muchii
            foreach (var edge in eg.ExpandedEdges)
            {
                int u = nodeIndex[edge.From];
                int v = nodeIndex[edge.To];

                AddEdge(u, v, edge.Capacity);
            }
        }

        private void AddEdge(int from, int to, int capacity)
        {
            var forward = new FlowEdge(to, capacity);
            var backward = new FlowEdge(from, 0);

            forward.Reverse = backward;
            backward.Reverse = forward;

            graph[from].Add(forward);
            graph[to].Add(backward);
        }

        private int BFS(int s, int t, int[] parent, FlowEdge[] parentEdge)
        {
            Array.Fill(parent, -1);
            parent[s] = -2;

            Queue<(int node, int flow)> q = new();
            q.Enqueue((s, int.MaxValue));

            while (q.Count > 0)
            {
                var (cur, flow) = q.Dequeue();

                foreach (var edge in graph[cur])
                {
                    if (parent[edge.To] == -1 && edge.ResidualCapacity() > 0)
                    {
                        parent[edge.To] = cur;
                        parentEdge[edge.To] = edge;

                        int newFlow = Math.Min(flow, edge.ResidualCapacity());

                        if (edge.To == t)
                            return newFlow;

                        q.Enqueue((edge.To, newFlow));
                    }
                }
            }

            return 0;
        }

        private int EdmondsKarp(int s, int t)
        {
            int flow = 0;
            int n = graph.Count;

            int[] parent = new int[n];
            FlowEdge[] parentEdge = new FlowEdge[n];

            int newFlow;

            while ((newFlow = BFS(s, t, parent, parentEdge)) > 0)
            {
                flow += newFlow;

                int cur = t;

                while (cur != s)
                {
                    var edge = parentEdge[cur];

                    edge.Flow += newFlow;
                    edge.Reverse.Flow -= newFlow;

                    cur = parent[cur];
                }
            }

            return flow;
        }
    }
}
