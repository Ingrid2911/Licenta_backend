using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    public class NodeRenderer
    {
        private const double NodeRadius = 15;
        private static readonly Brush NodeLabelBrush = Brushes.White;
        public void DrawAll(ExpandedGraph graph, Dictionary<string, Point> positions, Canvas canvas)
        {
            foreach (var kvp in positions)
            {
                string node = kvp.Key;
                Point pos = kvp.Value;

                DrawNode(graph, node, pos, canvas);
            }
        }
        private void DrawNode(ExpandedGraph graph, string node, Point pos, Canvas canvas)
        {
            Brush fill = GetNodeFill(node, graph);

            var ellipse = new Ellipse
            {
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Stroke = Brushes.Black,
                StrokeThickness = 1.4,
                Fill = fill
            };

            Canvas.SetLeft(ellipse, pos.X - NodeRadius);
            Canvas.SetTop(ellipse, pos.Y - NodeRadius);
            Panel.SetZIndex(ellipse, 10);
            canvas.Children.Add(ellipse);

            var labelBox = LabelFactory.CreateLabel(node, NodeLabelBrush, 10);
            labelBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size labelSize = labelBox.DesiredSize;

            Canvas.SetLeft(labelBox, pos.X - labelSize.Width / 2);
            Canvas.SetTop(labelBox, pos.Y + NodeRadius + 2);
            Panel.SetZIndex(labelBox, 20);
            canvas.Children.Add(labelBox);
        }
        private Brush GetNodeFill(string node, ExpandedGraph graph)
        {
            if (node == graph.SuperSource)
                return Brushes.LightGreen;

            if (node == graph.SuperSink)
                return Brushes.Orange;

            string original = NodeTypeHelper.IsTimeExpandedNode(node)
                ? NodeTypeHelper.GetOriginalNodeId(node)
                : node;

            if (original.StartsWith("S", StringComparison.OrdinalIgnoreCase) ||
                original.StartsWith("START", StringComparison.OrdinalIgnoreCase) ||
                original.Equals("s", StringComparison.OrdinalIgnoreCase))
            {
                return Brushes.LightGreen;
            }

            if (original.StartsWith("EXIT", StringComparison.OrdinalIgnoreCase) ||
                original.StartsWith("T", StringComparison.OrdinalIgnoreCase) ||
                original.Equals("t", StringComparison.OrdinalIgnoreCase))
            {
                return Brushes.Orange;
            }

            return Brushes.LightYellow;
        }
    }
}