using System.Collections.Generic;
using System.Linq;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.Models.Utilities
{
    public static class EvacuationPathExtractor
    {
        public static List<string> ExtractPath(
            List<(int from, FlowEdge edge)> flowEdges,
            Dictionary<int, string> indexToNode,
            SimpleGraph simpleGraph)
        {
            if (flowEdges == null || indexToNode == null || simpleGraph == null)
                return new List<string>();

            var adjacency = new Dictionary<string, HashSet<string>>();

            foreach (var (fromIdx, edge) in flowEdges)
            {
                if (edge.Flow <= 0) continue;
                if (!indexToNode.ContainsKey(fromIdx)) continue;
                if (!indexToNode.ContainsKey(edge.To)) continue;

                string fromExpanded = indexToNode[fromIdx];
                string toExpanded = indexToNode[edge.To];

                if (fromExpanded == "S*" || toExpanded == "T*") continue;
                if (fromExpanded == "S*_virtual" || toExpanded == "T*_virtual") continue;

                string fromOrig = StripTimeIndex(fromExpanded);
                string toOrig = StripTimeIndex(toExpanded);

                if (fromOrig == toOrig) continue; 

                if (!adjacency.ContainsKey(fromOrig))
                    adjacency[fromOrig] = new HashSet<string>();

                adjacency[fromOrig].Add(toOrig);
            }

            if (adjacency.Count == 0)
                return new List<string>();

            string source = simpleGraph.SourceNode;
            string sink = simpleGraph.SinkNode;

            return BFS(adjacency, source, sink);
        }

        private static string StripTimeIndex(string expandedNode)
        {
            int lastUnderscore = expandedNode.LastIndexOf('_');
            if (lastUnderscore < 0) return expandedNode;

            string suffix = expandedNode.Substring(lastUnderscore + 1);
            if (int.TryParse(suffix, out _))
                return expandedNode.Substring(0, lastUnderscore);

            return expandedNode;
        }

        private static List<string> BFS(
            Dictionary<string, HashSet<string>> adj,
            string source,
            string sink)
        {
            var visited = new HashSet<string>();
            var parent = new Dictionary<string, string?>();
            var queue = new Queue<string>();

            queue.Enqueue(source);
            visited.Add(source);
            parent[source] = null;

            while (queue.Count > 0)
            {
                string cur = queue.Dequeue();

                if (cur == sink)
                    return ReconstructPath(parent, source, sink);

                if (!adj.ContainsKey(cur)) continue;

                foreach (var neighbor in adj[cur])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        parent[neighbor] = cur;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return new List<string>();
        }

        private static List<string> ReconstructPath(
            Dictionary<string, string?> parent,
            string source,
            string sink)
        {
            var path = new List<string>();
            string? cur = sink;

            while (cur != null)
            {
                path.Add(cur);
                parent.TryGetValue(cur, out cur);
            }

            path.Reverse();
            return path;
        }
    }
}
