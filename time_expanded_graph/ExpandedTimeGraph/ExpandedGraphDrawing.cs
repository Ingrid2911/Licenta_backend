using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal class ExpandedGraphDrawing
    {
            private const double NodeRadius = 12;
            private const double SpacingX = 70;
            private const double SpacingY = 100;

            private const double StartX = 100;
            private const double StartY = 300;

            public void Draw(ExpandedGraph graph, Canvas canvas)
            {
                canvas.Children.Clear();

                // 1️⃣ Extragem nodurile originale (s, a, b, t)
                var originalNodes = graph.ExpandedNodes
                    .Where(n => n.Contains("_"))
                    .Select(n => n.Split('_')[0])
                    .Distinct()
                    .ToList();

                // adăugăm super-noduri
                originalNodes.Insert(0, graph.SuperSource);
                originalNodes.Add(graph.SuperSink);

                // mapare nod original -> index pe axa X
                Dictionary<string, int> xIndex = new();
                for (int i = 0; i < originalNodes.Count; i++)
                    xIndex[originalNodes[i]] = i;

                // 2️⃣ Calculăm pozițiile nodurilor extinse
                Dictionary<string, Point> positions = new();

                foreach (var node in graph.ExpandedNodes)
                {
                    if (node == graph.SuperSource || node == graph.SuperSink)
                        continue;

                    var parts = node.Split('_');
                    string original = parts[0];
                    int time = int.Parse(parts[1]);

                    double x = StartX + xIndex[original] * SpacingX;
                    double y = StartY + (graph.TimeHorizon - time) * SpacingY;

                    positions[node] = new Point(x, y);
                }

                // poziții pentru S* și T*
                double middleY = StartY + (graph.TimeHorizon - 1) * SpacingY / 2;

                positions[graph.SuperSource] =
                    new Point(StartX - SpacingX, middleY);

                positions[graph.SuperSink] =
                    new Point(StartX + xIndex.Count * SpacingX, middleY);

                // 3️⃣ Desenăm muchiile
                foreach (var edge in graph.ExpandedEdges)
                {
                    if (!positions.ContainsKey(edge.From) ||
                        !positions.ContainsKey(edge.To))
                        continue;

                    Point from = positions[edge.From];
                    Point to = positions[edge.To];

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

                // 4️⃣ Desenăm nodurile
                foreach (var kvp in positions)
                {
                    string node = kvp.Key;
                    Point pos = kvp.Value;

                    Ellipse ellipse = new Ellipse
                    {
                        Width = NodeRadius * 2,
                        Height = NodeRadius * 2,
                        Stroke = Brushes.Black,
                        Fill = Brushes.LightYellow,
                        StrokeThickness = 2
                    };

                    Canvas.SetLeft(ellipse, pos.X - NodeRadius);
                    Canvas.SetTop(ellipse, pos.Y - NodeRadius);
                    canvas.Children.Add(ellipse);

                    TextBlock label = new TextBlock
                    {
                        Text = node,
                        FontSize = 10,
                        Foreground = Brushes.Black
                    };

                    Canvas.SetLeft(label, pos.X - 10);
                    Canvas.SetTop(label, pos.Y - 25);
                    canvas.Children.Add(label);
                }
            }
        }
    }
