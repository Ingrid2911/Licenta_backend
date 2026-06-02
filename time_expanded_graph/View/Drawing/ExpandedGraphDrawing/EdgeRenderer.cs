using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    public class EdgeRenderer
    {
        private static readonly Brush EdgeLabelBrush = Brushes.White;

        public void DrawAll(
            List<ExpandedEdge> edges,
            Dictionary<string, Point> positions,
            Canvas canvas,
            HashSet<string>? hiddenCapacityLabels)
        {
            foreach (var edge in edges)
            {
                if (!positions.ContainsKey(edge.From) || !positions.ContainsKey(edge.To))
                    continue;

                DrawEdge(edge, positions, canvas, hiddenCapacityLabels);
            }
        }
        private void DrawEdge(
            ExpandedEdge edge,
            Dictionary<string, Point> positions,
            Canvas canvas,
            HashSet<string>? hiddenCapacityLabels)
        {
            Point from = positions[edge.From];
            Point to = positions[edge.To];

            Brush color = edge.Type == ExpandedEdgeType.Holdover
                ? Brushes.Gray
                : Brushes.DodgerBlue;

            DoubleCollection? dash = edge.Type == ExpandedEdgeType.Holdover
                ? new DoubleCollection { 4, 4 }
                : null;
            var line = new Line
            {
                X1 = from.X,
                Y1 = from.Y,
                X2 = to.X,
                Y2 = to.Y,
                Stroke = color,
                StrokeThickness = edge.Type == ExpandedEdgeType.Holdover ? 1.1 : 1.6,
                StrokeDashArray = dash,
                Opacity = edge.Type == ExpandedEdgeType.Holdover ? 0.55 : 0.95
            };

            Panel.SetZIndex(line, 1);
            canvas.Children.Add(line);

            string edgeKey = $"{edge.From}->{edge.To}";
            bool hideCapacityLabel = hiddenCapacityLabels != null && hiddenCapacityLabels.Contains(edgeKey);

            if (edge.Type != ExpandedEdgeType.Holdover && !hideCapacityLabel)
            {
                var capLabel = LabelFactory.CreateLabel(edge.Capacity.ToString(), EdgeLabelBrush, 10);

                LabelFactory.AddEdgeLabel(
                    canvas: canvas,
                    labelBox: capLabel,
                    from: from,
                    to: to,
                    fromNode: edge.From,
                    toNode: edge.To,
                    guideBrush: Brushes.DodgerBlue,
                    t: 0.45,
                    offset: 14,
                    zIndex: 12);
            }
        }
    }
}