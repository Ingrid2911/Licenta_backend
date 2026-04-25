using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.MaxFlowAlgorithms;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal class ExpandedGraphDrawing
    {
        private const double NodeRadius = 12;
        private const double SpacingX = 100;
        private const double SpacingY = 80;

        private const double StartX = 120;
        private const double StartY = 350;

        private Dictionary<string, Point> positions;

        // 🔴 metoda principală cu flow
        public void DrawWithFlow(
            ExpandedGraph graph,
            Canvas canvas,
            List<(int from, FlowEdge edge)> flowEdges,
            Dictionary<int, string> indexMap)
        {
            Draw(graph, canvas);

            foreach (var fe in flowEdges)
            {
                if (fe.edge.Flow <= 0) continue;

                string fromNode = indexMap[fe.from];
                string toNode = indexMap[fe.edge.To];

                if (!positions.ContainsKey(fromNode) || !positions.ContainsKey(toNode))
                    continue;

                Point from = positions[fromNode];
                Point to = positions[toNode];

                // 🔴 linie flow
                Line flowLine = new Line
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 3
                };

                canvas.Children.Add(flowLine);

                // 🔢 flow/cap
                double midX = (from.X + to.X) / 2;
                double midY = (from.Y + to.Y) / 2;

                TextBlock label = new TextBlock
                {
                    Text = $"{fe.edge.Flow}/{fe.edge.Capacity}",
                    FontSize = 9,
                    Background = Brushes.White,
                    Foreground = Brushes.Red
                };

                Canvas.SetLeft(label, midX - 10);
                Canvas.SetTop(label, midY - 10);

                canvas.Children.Add(label);
            }
        }

        // 🔵 desen normal
        public void Draw(ExpandedGraph graph, Canvas canvas)
        {
            canvas.Children.Clear();
            positions = new Dictionary<string, Point>();

            // 🔹 calcul poziții
            foreach (var node in graph.ExpandedNodes)
            {
                // 🔥 ignorăm S* și T*
                if (!node.Contains("_"))
                    continue;

                var parts = node.Split('_');

                string original = parts[0];
                int time = int.Parse(parts.Last());

                double x = StartX + (original[0] - 'a' + 1) * SpacingX;

                if (original == "s") x = StartX;
                if (original == "t") x = StartX + 6 * SpacingX;

                double y = StartY + (graph.TimeHorizon - time) * SpacingY;

                positions[node] = new Point(x, y);
            }

            // 🔹 S* și T*
            double middleY = StartY + (graph.TimeHorizon * SpacingY) / 2;

            positions[graph.SuperSource] = new Point(StartX - SpacingX, middleY);
            positions[graph.SuperSink] = new Point(StartX + 7 * SpacingX, middleY);

            // 🔹 muchii
            foreach (var edge in graph.ExpandedEdges)
            {
                if (!positions.ContainsKey(edge.From) ||
                    !positions.ContainsKey(edge.To))
                    continue;

                Point from = positions[edge.From];
                Point to = positions[edge.To];

                Brush color = edge.Type == ExpandedEdgeType.Holdover
                    ? Brushes.Gray
                    : Brushes.DodgerBlue;

                DoubleCollection dash = edge.Type == ExpandedEdgeType.Holdover
                    ? new DoubleCollection { 4, 4 }
                    : null;

                Line line = new Line
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = color,
                    StrokeThickness = 1.5,
                    StrokeDashArray = dash
                };

                canvas.Children.Add(line);

                // 🔢 capacitate (doar movement)
                if (edge.Type != ExpandedEdgeType.Holdover)
                {
                    double midX = (from.X + to.X) / 2;
                    double midY = (from.Y + to.Y) / 2;

                    TextBlock capLabel = new TextBlock
                    {
                        Text = edge.Capacity.ToString(),
                        FontSize = 8,
                        Background = Brushes.White,
                        Foreground = Brushes.DarkRed
                    };

                    Canvas.SetLeft(capLabel, midX - 10);
                    Canvas.SetTop(capLabel, midY - 10);

                    canvas.Children.Add(capLabel);
                }
            }

            // 🔹 noduri
            foreach (var kvp in positions)
            {
                string node = kvp.Key;
                Point pos = kvp.Value;

                Brush fill = Brushes.LightYellow;

                if (node == graph.SuperSource) fill = Brushes.LightGreen;
                if (node == graph.SuperSink) fill = Brushes.Orange;

                Ellipse ellipse = new Ellipse
                {
                    Width = NodeRadius * 2,
                    Height = NodeRadius * 2,
                    Stroke = Brushes.Black,
                    Fill = fill
                };

                Canvas.SetLeft(ellipse, pos.X - NodeRadius);
                Canvas.SetTop(ellipse, pos.Y - NodeRadius);

                canvas.Children.Add(ellipse);

                // etichetă
                Border labelBox = new Border
                {
                    Background = Brushes.White,
                    Padding = new Thickness(2),

                    Child = new TextBlock
                    {
                        Text = node,
                        FontSize = 8
                    }
                };

                Canvas.SetLeft(labelBox, pos.X - 15);
                Canvas.SetTop(labelBox, pos.Y + 12);

                canvas.Children.Add(labelBox);
            }
        }
    }
}