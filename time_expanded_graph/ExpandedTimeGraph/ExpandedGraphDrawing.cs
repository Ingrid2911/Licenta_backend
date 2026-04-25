using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal class ExpandedGraphDrawing
    {
        private const double NodeRadius = 12;
        private const double SpacingX = 100;
        private const double SpacingY = 80;

        private const double StartX = 120;
        private const double StartY = 350;

        public void Draw(ExpandedGraph graph, Canvas canvas)
        {
            canvas.Children.Clear();

            // 🔹 1. ordonare noduri (coloane fixe)
            var originalNodes = graph.ExpandedNodes
                .Where(n => n.Contains("_"))
                .Select(n =>
                {
                    var parts = n.Split('_');
                    return string.Join("_", parts.Take(parts.Length - 1));
                })
                .Distinct()
                .OrderBy(n =>
                    n == "s" ? 0 :
                    n == "t" ? 100 :
                    n[0] // a, b, c...
                )
                .ToList();

            originalNodes.Insert(0, graph.SuperSource);
            originalNodes.Add(graph.SuperSink);

            Dictionary<string, int> xIndex = new();
            for (int i = 0; i < originalNodes.Count; i++)
                xIndex[originalNodes[i]] = i;

            // 🔹 2. poziții
            Dictionary<string, Point> positions = new();

            foreach (var node in graph.ExpandedNodes)
            {
                if (node == graph.SuperSource || node == graph.SuperSink)
                    continue;

                var parts = node.Split('_');

                string original = string.Join("_", parts.Take(parts.Length - 1));
                int time = int.Parse(parts.Last());

                double x = StartX + xIndex[original] * SpacingX;
                double y = StartY + (graph.TimeHorizon - time) * SpacingY;

                positions[node] = new Point(x, y);
            }

            double middleY = StartY + (graph.TimeHorizon * SpacingY) / 2;

            positions[graph.SuperSource] =
                new Point(StartX - SpacingX, middleY);

            positions[graph.SuperSink] =
                new Point(StartX + xIndex.Count * SpacingX, middleY);

            // 🔹 3. etichete timp
            for (int t = 0; t <= graph.TimeHorizon; t++)
            {
                double y = StartY + (graph.TimeHorizon - t) * SpacingY;

                TextBlock label = new TextBlock
                {
                    Text = $"t={t}",
                    FontSize = 10,
                    Foreground = Brushes.Gray
                };

                Canvas.SetLeft(label, 20);
                Canvas.SetTop(label, y - 5);

                canvas.Children.Add(label);
            }

            // 🔹 4. MUCHII + CAPACITĂȚI
            foreach (var edge in graph.ExpandedEdges)
            {
                if (!positions.ContainsKey(edge.From) ||
                    !positions.ContainsKey(edge.To))
                    continue;

                Point from = positions[edge.From];
                Point to = positions[edge.To];

                Brush color;
                DoubleCollection dash = null;

                if (edge.Type == ExpandedEdgeType.Holdover)
                {
                    color = Brushes.Gray;
                    dash = new DoubleCollection() { 4, 4 };
                }
                else
                {
                    color = Brushes.DodgerBlue;
                }

                // 🔹 desen muchie
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

                // 🔹 CAPACITATE (NU pentru holdover)
                if (edge.Type != ExpandedEdgeType.Holdover)
                {
                    double midX = (from.X + to.X) / 2;
                    double midY = (from.Y + to.Y) / 2;

                    Border capContainer = new Border
                    {
                        Background = Brushes.White,
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(2),
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(0.5),

                        Child = new TextBlock
                        {
                            Text = edge.Capacity.ToString(),
                            FontSize = 8,
                            Foreground = Brushes.DarkRed,
                            FontWeight = FontWeights.SemiBold
                        }
                    };

                    Canvas.SetLeft(capContainer, midX - 10);
                    Canvas.SetTop(capContainer, midY - 10);

                    canvas.Children.Add(capContainer);
                }
            }

            // 🔹 5. noduri + etichete
            foreach (var kvp in positions)
            {
                string node = kvp.Key;
                Point pos = kvp.Value;

                Brush fill = Brushes.LightYellow;

                if (node == graph.SuperSource)
                    fill = Brushes.LightGreen;
                else if (node == graph.SuperSink)
                    fill = Brushes.Orange;

                Ellipse ellipse = new Ellipse
                {
                    Width = NodeRadius * 2,
                    Height = NodeRadius * 2,
                    Stroke = Brushes.Black,
                    Fill = fill,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(ellipse, pos.X - NodeRadius);
                Canvas.SetTop(ellipse, pos.Y - NodeRadius);
                canvas.Children.Add(ellipse);

                // 🔹 etichetă simplificată (fără clutter)
                var parts = node.Split('_');
                string original = string.Join("_", parts.Take(parts.Length - 1));
                string time = parts.Last();

                Border labelContainer = new Border
                {
                    Background = Brushes.White,
                    Padding = new Thickness(2),

                    Child = new TextBlock
                    {
                        Text = $"{original}_{time}",
                        FontSize = 8
                    }
                };

                Canvas.SetLeft(labelContainer, pos.X - 18);
                Canvas.SetTop(labelContainer, pos.Y + 12);

                canvas.Children.Add(labelContainer);
            }
        }
    }
}