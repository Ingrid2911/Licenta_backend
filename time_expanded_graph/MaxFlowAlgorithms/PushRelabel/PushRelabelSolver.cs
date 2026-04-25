using System;
using System.Collections.Generic;
using System.Linq;
using time_expanded_graph.ExpandedTimeGraph;
using time_expanded_graph.MaxFlowAlgorithms;

namespace time_expanded_graph.MaxFlowAlgorithms.PushRelabel
{
    internal class PushRelabelSolver
    {
        private List<List<FlowEdge>> graph;
        private Dictionary<string, int> nodeIndex;

        private int[] height;
        private int[] excess;
        private int[] current;

        private bool[] active;
        private Queue<int> activeNodes;

        private int source;
        private int sink;

        public PushRelabelSolver(ExpandedGraph eg)
        {
            BuildGraph(eg);
        }

        public int ComputeMaxFlow(string sourceNode, string sinkNode)
        {
            source = nodeIndex[sourceNode];
            sink = nodeIndex[sinkNode];

            return PushRelabel();
        }

        // 🔹 Build graph
        private void BuildGraph(ExpandedGraph eg)
        {
            nodeIndex = new Dictionary<string, int>();
            graph = new List<List<FlowEdge>>();

            var allNodes = new HashSet<string>();

            foreach (var e in eg.ExpandedEdges)
            {
                allNodes.Add(e.From);
                allNodes.Add(e.To);
            }

            allNodes.Add(eg.SuperSource);
            allNodes.Add(eg.SuperSink);

            int idx = 0;
            foreach (var node in allNodes)
            {
                nodeIndex[node] = idx++;
                graph.Add(new List<FlowEdge>());
            }

            foreach (var e in eg.ExpandedEdges)
            {
                AddEdge(nodeIndex[e.From], nodeIndex[e.To], e.Capacity);
            }
        }

        private void AddEdge(int from, int to, int capacity)
        {
            var f = new FlowEdge(to, capacity);
            var r = new FlowEdge(from, 0);

            f.Reverse = r;
            r.Reverse = f;

            graph[from].Add(f);
            graph[to].Add(r);
        }

        // 🔹 MAIN
        private int PushRelabel()
        {
            int n = graph.Count;

            height = new int[n];
            excess = new int[n];
            current = new int[n];
            active = new bool[n];
            activeNodes = new Queue<int>();

            GlobalRelabel();

            height[source] = n;

            // 🔥 preflow
            foreach (var e in graph[source])
            {
                int flow = e.Capacity;

                e.Flow += flow;
                e.Reverse.Flow -= flow;

                excess[source] -= flow;
                excess[e.To] += flow;

                Enqueue(e.To);
            }

            while (activeNodes.Count > 0)
            {
                int u = activeNodes.Dequeue();
                active[u] = false;

                if (u == source || u == sink)
                    continue;

                Discharge(u);

                if (excess[u] > 0)
                    Enqueue(u);
            }

            return excess[sink];
        }

        // 🔹 discharge
        private void Discharge(int u)
        {
            while (excess[u] > 0)
            {
                if (current[u] == graph[u].Count)
                {
                    Relabel(u);
                    current[u] = 0;
                }
                else
                {
                    var e = graph[u][current[u]];

                    if (e.ResidualCapacity() > 0 &&
                        height[u] == height[e.To] + 1)
                    {
                        Push(u, e);
                    }
                    else
                    {
                        current[u]++;
                    }
                }
            }
        }

        // 🔹 push
        private void Push(int u, FlowEdge e)
        {
            int send = Math.Min(excess[u], e.ResidualCapacity());

            if (send <= 0) return;

            e.Flow += send;
            e.Reverse.Flow -= send;

            excess[u] -= send;
            excess[e.To] += send;

            Enqueue(e.To);
        }

        // 🔹 relabel
        private void Relabel(int u)
        {
            int min = int.MaxValue;

            foreach (var e in graph[u])
            {
                if (e.ResidualCapacity() > 0)
                    min = Math.Min(min, height[e.To]);
            }

            if (min < int.MaxValue)
                height[u] = min + 1;
        }

        // 🔹 global relabel
        private void GlobalRelabel()
        {
            Array.Fill(height, int.MaxValue);

            height[sink] = 0;

            Queue<int> q = new();
            q.Enqueue(sink);

            while (q.Count > 0)
            {
                int u = q.Dequeue();

                foreach (var e in graph[u])
                {
                    if (e.Reverse.ResidualCapacity() > 0 &&
                        height[e.To] == int.MaxValue)
                    {
                        height[e.To] = height[u] + 1;
                        q.Enqueue(e.To);
                    }
                }
            }
        }

        private void Enqueue(int v)
        {
            if (!active[v] && excess[v] > 0 && v != source && v != sink)
            {
                active[v] = true;
                activeNodes.Enqueue(v);
            }
        }

        // 🔥 🔥 🔥 IMPORTANT – pentru vizualizare 🔥 🔥 🔥

        public List<(int from, FlowEdge edge)> GetAllEdges()
        {
            var result = new List<(int, FlowEdge)>();

            for (int i = 0; i < graph.Count; i++)
            {
                foreach (var e in graph[i])
                {
                    if (e.Capacity > 0)
                        result.Add((i, e));
                }
            }

            return result;
        }

        public Dictionary<int, string> GetIndexToNodeMap()
        {
            return nodeIndex.ToDictionary(x => x.Value, x => x.Key);
        }
    }
}