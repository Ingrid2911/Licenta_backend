using System;
using System.Collections.Generic;
using time_expanded_graph.ExpandedTimeGraph;

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
            if (!nodeIndex.ContainsKey(sourceNode))
                throw new ArgumentException($"Nodul sursă '{sourceNode}' nu există.");

            if (!nodeIndex.ContainsKey(sinkNode))
                throw new ArgumentException($"Nodul destinație '{sinkNode}' nu există.");

            source = nodeIndex[sourceNode];
            sink = nodeIndex[sinkNode];

            return PushRelabel();
        }

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
                AddEdge(
                    nodeIndex[edge.From],
                    nodeIndex[edge.To],
                    edge.Capacity
                );
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

            foreach (var edge in graph[source])
            {
                if (edge.Capacity > 0)
                {
                    int flow = edge.Capacity;

                    edge.Flow += flow;
                    edge.Reverse.Flow -= flow;

                    excess[source] -= flow;
                    excess[edge.To] += flow;

                    Enqueue(edge.To);
                }
            }

            int operations = 0;
            int globalRelabelFrequency = Math.Max(1, n);

            while (activeNodes.Count > 0)
            {
                int u = activeNodes.Dequeue();
                active[u] = false;

                if (u == source || u == sink)
                    continue;

                Discharge(u);

                operations++;

                if (operations % globalRelabelFrequency == 0)
                {
                    GlobalRelabel();
                    Array.Fill(current, 0);
                }

                if (excess[u] > 0)
                    Enqueue(u);
            }

            return excess[sink];
        }

        private void Discharge(int u)
        {
            while (excess[u] > 0)
            {
                if (height[u] == int.MaxValue)
                    break;

                if (current[u] == graph[u].Count)
                {
                    Relabel(u);
                    current[u] = 0;
                }
                else
                {
                    FlowEdge edge = graph[u][current[u]];

                    if (edge.ResidualCapacity() > 0 &&
                        height[u] == height[edge.To] + 1)
                    {
                        Push(u, edge);
                    }
                    else
                    {
                        current[u]++;
                    }
                }
            }
        }

        private void Push(int u, FlowEdge edge)
        {
            int v = edge.To;

            int send = Math.Min(excess[u], edge.ResidualCapacity());

            if (send <= 0)
                return;

            edge.Flow += send;
            edge.Reverse.Flow -= send;

            excess[u] -= send;
            excess[v] += send;

            Enqueue(v);
        }

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

            if (minHeight == int.MaxValue)
            {
                height[u] = int.MaxValue;
            }
            else
            {
                height[u] = minHeight + 1;
            }
        }

        private void GlobalRelabel()
        {
            Array.Fill(height, int.MaxValue);

            height[sink] = 0;

            var queue = new Queue<int>();
            queue.Enqueue(sink);

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();

                foreach (var edge in graph[u])
                {
                    FlowEdge reverse = edge.Reverse;

                    if (reverse.ResidualCapacity() > 0 &&
                        height[edge.To] == int.MaxValue)
                    {
                        height[edge.To] = height[u] + 1;
                        queue.Enqueue(edge.To);
                    }
                }
            }
        }

        private void Enqueue(int node)
        {
            if (node != source &&
                node != sink &&
                !active[node] &&
                excess[node] > 0)
            {
                active[node] = true;
                activeNodes.Enqueue(node);
            }
        }
    }
}