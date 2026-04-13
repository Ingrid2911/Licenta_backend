using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using time_expanded_graph.ExpandedTimeGraph;

namespace time_expanded_graph.MaxFlowAlgorithms.PushRelabel
{
    internal class PushRelabelSolver
    {
        private List<List<FlowEdge>> graph;
        private Dictionary<string, int> nodeIndex;

        private int[] height;
        private int[] excess;

        public PushRelabelSolver(ExpandedGraph eg)
        {
            BuildGraph(eg);
        }

        public int ComputeMaxFlow(string source, string sink)
        {
            int s = nodeIndex[source];
            int t = nodeIndex[sink];

            return PushRelabel(s, t);
        }

        // 🔷 BuildGraph (identic)
        private void BuildGraph(ExpandedGraph eg)
        {
            nodeIndex = new Dictionary<string, int>();
            graph = new List<List<FlowEdge>>();

            var allNodes = new HashSet<string>();

            foreach (var edge in eg.ExpandedEdges)
            {
                allNodes.Add(edge.From);
                allNodes.Add(edge.To);
            }

            allNodes.Add(eg.SuperSource);
            allNodes.Add(eg.SuperSink);

            int index = 0;
            foreach (var node in allNodes)
            {
                nodeIndex[node] = index++;
                graph.Add(new List<FlowEdge>());
            }

            foreach (var edge in eg.ExpandedEdges)
            {
                AddEdge(nodeIndex[edge.From], nodeIndex[edge.To], edge.Capacity);
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

        // 🔷 PUSH
        private void Push(int u, FlowEdge edge)
        {
            int v = edge.To;
            int send = Math.Min(excess[u], edge.ResidualCapacity());

            if (send <= 0 || height[u] != height[v] + 1)
                return;

            edge.Flow += send;
            edge.Reverse.Flow -= send;

            excess[u] -= send;
            excess[v] += send;
        }

        // 🔷 RELABEL
        private void Relabel(int u)
        {
            int minHeight = int.MaxValue;

            foreach (var edge in graph[u])
            {
                if (edge.ResidualCapacity() > 0)
                {
                    minHeight = Math.Min(minHeight, height[edge.To]);
                }
            }

            if (minHeight < int.MaxValue)
                height[u] = minHeight + 1;
        }

        // 🔷 Discharge (cheia algoritmului)
        private void Discharge(int u)
        {
            while (excess[u] > 0)
            {
                bool pushed = false;

                foreach (var edge in graph[u])
                {
                    if (edge.ResidualCapacity() > 0 &&
                        height[u] == height[edge.To] + 1)
                    {
                        Push(u, edge);
                        pushed = true;

                        if (excess[u] == 0)
                            break;
                    }
                }

                if (!pushed)
                {
                    Relabel(u);
                }
            }
        }

        // 🔷 Algoritmul principal
        private int PushRelabel(int s, int t)
        {
            int n = graph.Count;

            height = new int[n];
            excess = new int[n];

            height[s] = n;

            // 🔥 preflow inițial
            foreach (var edge in graph[s])
            {
                edge.Flow = edge.Capacity;
                edge.Reverse.Flow = -edge.Capacity;

                excess[edge.To] += edge.Capacity;
                excess[s] -= edge.Capacity;
            }

            // 🔷 lista noduri active
            var nodes = new List<int>();
            for (int i = 0; i < n; i++)
            {
                if (i != s && i != t)
                    nodes.Add(i);
            }

            int p = 0;

            while (p < nodes.Count)
            {
                int u = nodes[p];
                int oldHeight = height[u];

                Discharge(u);

                if (height[u] > oldHeight)
                {
                    nodes.RemoveAt(p);
                    nodes.Insert(0, u);
                    p = 0;
                }
                else
                {
                    p++;
                }
            }

            return excess[t];
        }
    }
}
