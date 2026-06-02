using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.Models.Building
{
    public static class BuildingPlanFactory
    {
        private const double StartX = 90;
        private const double StartY = 320;
        private const double ColumnSpacing = 260;
        private const double RowSpacing = 170;

        public static BuildingPlan FromSimpleGraph(SimpleGraph graph)
        {
            var plan = new BuildingPlan();

            if (graph == null || graph.Nodes.Count == 0)
                return plan;

            var levels = ComputeNodeLevels(graph);

            CreateElements(plan, graph, levels);
            CreateConnections(plan, graph);

            return plan;
        }

        private static Dictionary<string, int> ComputeNodeLevels(SimpleGraph graph)
        {
            var levels = new Dictionary<string, int>();

            foreach (var node in graph.Nodes)
                levels[node] = -1;

            string source = graph.SourceNode;

            if (!levels.ContainsKey(source))
                source = graph.Nodes.First();

            levels[source] = 0;

            var queue = new Queue<string>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                var outgoing = graph.Edges
                    .Where(e => e.From == current)
                    .Select(e => e.To)
                    .Distinct()
                    .ToList();

                foreach (var next in outgoing)
                {
                    if (!levels.ContainsKey(next))
                        continue;

                    if (levels[next] == -1)
                    {
                        levels[next] = levels[current] + 1;
                        queue.Enqueue(next);
                    }
                }
            }

            foreach (var node in graph.Nodes)
            {
                if (levels[node] == -1)
                    levels[node] = 1;
            }

            int maxLevelWithoutSink = levels
                .Where(kvp => kvp.Key != graph.SinkNode)
                .Select(kvp => kvp.Value)
                .DefaultIfEmpty(0)
                .Max();

            if (levels.ContainsKey(graph.SinkNode))
                levels[graph.SinkNode] = maxLevelWithoutSink + 1;

            return levels;
        }

        private static void CreateElements(
            BuildingPlan plan,
            SimpleGraph graph,
            Dictionary<string, int> levels)
        {
            var groupedByLevel = levels
                .GroupBy(kvp => kvp.Value)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedByLevel)
            {
                int level = group.Key;

                var nodesInLevel = group
                    .Select(kvp => kvp.Key)
                    .OrderBy(n => GetNodeVisualPriority(n, graph))
                    .ThenBy(n => n)
                    .ToList();

                double groupHeight = (nodesInLevel.Count - 1) * RowSpacing;
                double firstY = StartY - groupHeight / 2.0;

                for (int i = 0; i < nodesInLevel.Count; i++)
                {
                    string nodeId = nodesInLevel[i];

                    BuildingElementType type = GetElementType(nodeId, graph);

                    double x = StartX + level * ColumnSpacing;
                    double y = firstY + i * RowSpacing;

                    var element = new BuildingElement(
                        type,
                        new Point(x, y),
                        forcedId: nodeId);

                    element.Label = GetElementLabel(nodeId, graph, type);

                    plan.AddElement(element);
                }
            }
        }

        private static void CreateConnections(BuildingPlan plan, SimpleGraph graph)
        {
            var existingNodes = plan.Elements
                .Select(e => e.Id)
                .ToHashSet();

            var added = new HashSet<string>();

            foreach (var edge in graph.Edges)
            {
                if (!existingNodes.Contains(edge.From) || !existingNodes.Contains(edge.To))
                    continue;

                string directKey = $"{edge.From}->{edge.To}";
                string reverseKey = $"{edge.To}->{edge.From}";

                if (added.Contains(directKey) || added.Contains(reverseKey))
                    continue;

                var reverseEdge = graph.Edges.FirstOrDefault(e =>
                    e.From == edge.To &&
                    e.To == edge.From);

                bool isBidirectional = reverseEdge != null;

                int capacity = edge.Capacity;
                int travelTime = edge.TravelTime;

                if (reverseEdge != null)
                {
                    capacity = Math.Min(edge.Capacity, reverseEdge.Capacity);
                    travelTime = Math.Min(edge.TravelTime, reverseEdge.TravelTime);
                }

                plan.AddConnection(new HallwayConnection(
                    edge.From,
                    edge.To,
                    capacity,
                    travelTime,
                    isBidirectional));

                added.Add(directKey);

                if (isBidirectional)
                    added.Add(reverseKey);
            }
        }

        private static BuildingElementType GetElementType(string nodeId, SimpleGraph graph)
        {
            if (nodeId == graph.SourceNode ||
                nodeId.Equals("s", StringComparison.OrdinalIgnoreCase) ||
                nodeId.StartsWith("S_", StringComparison.OrdinalIgnoreCase) ||
                nodeId.StartsWith("START", StringComparison.OrdinalIgnoreCase))
            {
                return BuildingElementType.StartPoint;
            }

            if (nodeId == graph.SinkNode ||
                nodeId.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                nodeId.StartsWith("T_", StringComparison.OrdinalIgnoreCase) ||
                nodeId.StartsWith("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                return BuildingElementType.ExitDoor;
            }

            if (nodeId.StartsWith("SC", StringComparison.OrdinalIgnoreCase) ||
                nodeId.StartsWith("STAIR", StringComparison.OrdinalIgnoreCase))
            {
                return BuildingElementType.Stairs;
            }

            if (nodeId.StartsWith("LF", StringComparison.OrdinalIgnoreCase) ||
                nodeId.StartsWith("LIFT", StringComparison.OrdinalIgnoreCase))
            {
                return BuildingElementType.Elevator;
            }

            return BuildingElementType.Room;
        }

        private static string GetElementLabel(
            string nodeId,
            SimpleGraph graph,
            BuildingElementType type)
        {
            if (type == BuildingElementType.StartPoint)
                return "Start";

            if (type == BuildingElementType.ExitDoor)
                return "Ieșire";

            if (type == BuildingElementType.Stairs)
                return "Scări";

            if (type == BuildingElementType.Elevator)
                return "Lift";

            return $"Camera {nodeId.ToUpper()}";
        }

        private static int GetNodeVisualPriority(string nodeId, SimpleGraph graph)
        {
            if (nodeId == graph.SourceNode)
                return 0;

            if (nodeId == graph.SinkNode)
                return 99;

            return 1;
        }
    }
}