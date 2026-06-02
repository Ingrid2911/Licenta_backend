using System.Windows;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    public class LayoutCalculator
    {
        private const double SpacingX = 150;
        private const double SpacingY = 95;
        private const double StartX = 180;
        private const double StartY = 120;
        public Dictionary<string, Point> CalculatePositions(ExpandedGraph graph)
        {
            var positions = new Dictionary<string, Point>();

            var originalNodeColumns = BuildOriginalNodeColumns(graph);
            BuildExpandedNodePositions(graph, originalNodeColumns, positions);
            AddSuperSourceAndSinkPositions(graph, originalNodeColumns.Count, positions);

            return positions;
        }
        private Dictionary<string, int> BuildOriginalNodeColumns(ExpandedGraph graph)
        {
            var originalNodes = graph.ExpandedNodes
                .Where(NodeTypeHelper.IsTimeExpandedNode)
                .Select(NodeTypeHelper.GetOriginalNodeId)
                .Distinct()
                .OrderBy(NodeTypeHelper.GetNodeTypePriority)
                .ThenBy(NodeTypeHelper.ExtractNumberFromNode)
                .ThenBy(n => n)
                .ToList();

            var columns = new Dictionary<string, int>();

            for (int i = 0; i < originalNodes.Count; i++)
                columns[originalNodes[i]] = i;

            return columns;
        }
        private void BuildExpandedNodePositions(
            ExpandedGraph graph,
            Dictionary<string, int> originalNodeColumns,
            Dictionary<string, Point> positions)
        {
            foreach (string node in graph.ExpandedNodes)
            {
                if (!NodeTypeHelper.IsTimeExpandedNode(node))
                    continue;

                string originalNodeId = NodeTypeHelper.GetOriginalNodeId(node);
                int time = NodeTypeHelper.GetTimeFromExpandedNode(node);

                if (!originalNodeColumns.ContainsKey(originalNodeId))
                    continue;

                int column = originalNodeColumns[originalNodeId];

                double x = StartX + column * SpacingX;
                double y = StartY + time * SpacingY;

                positions[node] = new Point(x, y);
            }
        }
        private void AddSuperSourceAndSinkPositions(
            ExpandedGraph graph,
            int originalNodeCount,
            Dictionary<string, Point> positions)
        {
            double maxY = StartY + graph.TimeHorizon * SpacingY;
            double middleY = StartY + (maxY - StartY) / 2;

            positions[graph.SuperSource] = new Point(
                StartX - SpacingX,
                middleY);

            positions[graph.SuperSink] = new Point(
                StartX + originalNodeCount * SpacingX,
                middleY);
        }
    }
}