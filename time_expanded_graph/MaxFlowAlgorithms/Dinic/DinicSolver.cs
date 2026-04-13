using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using time_expanded_graph.ExpandedTimeGraph;

namespace time_expanded_graph.MaxFlowAlgorithms.Dinic
{
    internal class DinicSolver
    {
        private List<List<FlowEdge>> graph;
        private Dictionary<string, int> nodeIndex;

        private int[] level;
        private int[] ptr;

        public DinicSolver(ExpandedGraph eg)
        {
            BuildGraph(eg);
        }

        public int ComputeMaxFlow(string source, string sink)
        {
            int s = nodeIndex[source];
            int t = nodeIndex[sink];

            return Dinic(s, t);
        }

        // 🔷 BuildGraph (identic ca la Edmonds-Karp)
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

        // 🔷 BFS (niveluri)
        private bool BFS(int s, int t)
        {
            Array.Fill(level, -1);
            level[s] = 0;

            Queue<int> q = new();
            q.Enqueue(s);

            while (q.Count > 0)
            {
                int v = q.Dequeue();

                foreach (var edge in graph[v])
                {
                    if (edge.ResidualCapacity() > 0 && level[edge.To] == -1)
                    {
                        level[edge.To] = level[v] + 1;
                        q.Enqueue(edge.To);
                    }
                }
            }

            return level[t] != -1;
        }

        // 🔷 DFS (trimite flow)
        private int DFS(int v, int t, int pushed)
        {
            if (pushed == 0) return 0;
            if (v == t) return pushed;

            for (; ptr[v] < graph[v].Count; ptr[v]++)
            {
                var edge = graph[v][ptr[v]];

                if (level[edge.To] != level[v] + 1 || edge.ResidualCapacity() <= 0)
                    continue;

                int tr = DFS(edge.To, t, Math.Min(pushed, edge.ResidualCapacity()));

                if (tr == 0) continue;

                edge.Flow += tr;
                edge.Reverse.Flow -= tr;

                return tr;
            }

            return 0;
        }

        // 🔷 Dinic principal
        private int Dinic(int s, int t)
        {
            int flow = 0;

            int n = graph.Count;
            level = new int[n];
            ptr = new int[n];

            while (BFS(s, t))
            {
                Array.Fill(ptr, 0);

                int pushed;
                while ((pushed = DFS(s, t, int.MaxValue)) > 0)
                {
                    flow += pushed;
                }
            }

            return flow;
        }
    }
}
