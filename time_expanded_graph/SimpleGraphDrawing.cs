using System;
using System.Collections.Generic;
using System.Linq;
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

            // 🔵 Layout pe niveluri, similar grafului extins
            var nodePositions = GenerateLinearLayout(graph, canvas);

            // 1️⃣ Muchii
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

                // Capacitate
                TextBlock capLabel = new TextBlock
                {
                    Text = edge.Capacity.ToString(),
                    FontSize = 10,
                    Foreground = Brushes.Red,
                    FontWeight = FontWeights.Bold
                };

                double midX = (from.X + to.X) / 2;
                double midY = (from.Y + to.Y) / 2;

                Canvas.SetLeft(capLabel, midX);
                Canvas.SetTop(capLabel, midY);

                canvas.Children.Add(capLabel);
            }

            // 2️⃣ Noduri
            foreach (var node in graph.Nodes)
            {
                if (!nodePositions.ContainsKey(node))
                    continue;

                Point pos = nodePositions[node];

                Brush fill = Brushes.LightGray;

                if (node == graph.SourceNode)
                   fill = Brushes.LightGreen;
                else if (node == graph.SinkNode)
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

        private Dictionary<string, Point> GenerateLinearLayout(SimpleGraph graph, Canvas canvas)
        {
            var positions = new Dictionary<string, Point>();

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            if (width == 0) width = 1400;
            if (height == 0) height = 700;

            var middleNodes = graph.Nodes
                .Where(n => n != graph.SourceNode && n != graph.SinkNode)
                .OrderBy(n => n)
                .ToList();

            // 🔵 Construim niveluri pe baza muchiilor
            var levels = new Dictionary<string, int>();
            levels[graph.SourceNode] = 0;

            Queue<string> queue = new();
            queue.Enqueue(graph.SourceNode);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                foreach (var edge in graph.Edges.Where(e => e.From == current))
                {
                    if (!levels.ContainsKey(edge.To))
                    {
                        levels[edge.To] = levels[current] + 1;
                        queue.Enqueue(edge.To);
                    }
                }
            }

            // Sink pe ultimul nivel
            int maxLevel = levels.Values.Max();
            levels[graph.SinkNode] = maxLevel;

            // Grupăm nodurile pe nivel
            var groupedLevels = levels
                .GroupBy(kvp => kvp.Value)
                .OrderBy(g => g.Key)
                .ToList();

            double xSpacing = width / (groupedLevels.Count + 1);

            foreach (var levelGroup in groupedLevels)
            {
                int level = levelGroup.Key;
                var nodesAtLevel = levelGroup
                    .Select(kvp => kvp.Key)
                    .OrderBy(n => n)
                    .ToList();

                double ySpacing = height / (nodesAtLevel.Count + 1);

                for (int i = 0; i < nodesAtLevel.Count; i++)
                {
                    double x = xSpacing * (level + 1);
                    double y = ySpacing * (i + 1);

                    positions[nodesAtLevel[i]] = new Point(x, y);
                }
            }

            return positions;
        }
    }
}