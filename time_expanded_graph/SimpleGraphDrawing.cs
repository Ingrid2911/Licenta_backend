using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace time_expanded_graph
{
    internal class SimpleGraphDrawing
    {
        private const double NodeRadius = 20;

        public void Draw(SimpleGraph graph, Canvas canvas)
        {
            canvas.Children.Clear();

            if (graph.Nodes.Count == 0)
                return;

            // 🔵 Generăm poziții automat
            var nodePositions = GenerateCircularLayout(graph.Nodes, canvas);

            // 1️⃣ Desenăm muchiile
            foreach (var edge in graph.Edges)
            {
                if (!nodePositions.ContainsKey(edge.From) ||
                    !nodePositions.ContainsKey(edge.To))
                    continue;

                Point from = nodePositions[edge.From];
                Point to = nodePositions[edge.To];

                Line line = new Line
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                canvas.Children.Add(line);
            }

            // 2️⃣ Desenăm nodurile
            foreach (var node in graph.Nodes)
            {
                if (!nodePositions.ContainsKey(node))
                    continue;

                Point pos = nodePositions[node];

                Ellipse ellipse = new Ellipse
                {
                    Width = NodeRadius * 2,
                    Height = NodeRadius * 2,
                    Stroke = Brushes.Black,
                    Fill = Brushes.LightGray,
                    StrokeThickness = 2
                };

                Canvas.SetLeft(ellipse, pos.X - NodeRadius);
                Canvas.SetTop(ellipse, pos.Y - NodeRadius);
                canvas.Children.Add(ellipse);

                TextBlock label = new TextBlock
                {
                    Text = node,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(label, pos.X - 6);
                Canvas.SetTop(label, pos.Y - 8);
                canvas.Children.Add(label);
            }
        }

        // 🔵 Layout automat pe cerc
        private Dictionary<string, Point> GenerateCircularLayout(List<string> nodes, Canvas canvas)
        {
            var positions = new Dictionary<string, Point>();

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            if (width == 0) width = 600;   // fallback dacă nu e încă randat
            if (height == 0) height = 400;

            double centerX = width / 2;
            double centerY = height / 2;
            double radius = Math.Min(width, height) / 3;

            int n = nodes.Count;

            for (int i = 0; i < n; i++)
            {
                double angle = 2 * Math.PI * i / n;

                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);

                positions[nodes[i]] = new Point(x, y);
            }

            return positions;
        }
    }
}