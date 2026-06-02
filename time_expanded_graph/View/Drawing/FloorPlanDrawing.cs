using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Drawing
{
    /// <summary>
    /// Renderer arhitectural pentru planul clădirii.
    ///
    /// Fiecare tip de element are simbolul său standard:
    ///   Room       → dreptunghi cu pereți groși + haşură ușoară + etichetă
    ///   Door       → deschidere în perete + arc 90° (foaia ușii)
    ///   Stairs     → dreptunghi gri cu linii paralele (trepte) + săgeată
    ///   Elevator   → dreptunghi albastru cu X în interior
    ///   ExitDoor   → dreptunghi portocaliu EXIT + săgeată
    ///   StartPoint → cerc verde cu S
    ///
    /// Conexiunile (holuri) sunt linii punctate între centrele elementelor.
    /// Ruta optimă este evidențiată cu verde închis, linie continuă.
    /// </summary>
    public class FloorPlanDrawing
    {
        // ─── Constante vizuale ────────────────────────────────────────────────────
        private const double WallThick = 5.0;   // grosimea peretelui camerei
        private const double HitPadding = 12.0;  // zona click în jurul elementului
        private const double StairsStep = 12.0;  // distanța dintre trepte

        // Culori arhitecturale
        private static readonly SolidColorBrush WallBrush = new(Color.FromRgb(44, 44, 42));
        private static readonly SolidColorBrush StairFill = new(Color.FromRgb(211, 209, 199));
        private static readonly SolidColorBrush StairLine = new(Color.FromRgb(136, 135, 128));
        private static readonly SolidColorBrush ElevFill = new(Color.FromRgb(181, 212, 244));
        private static readonly SolidColorBrush ElevStroke = new(Color.FromRgb(55, 138, 221));
        private static readonly SolidColorBrush ExitFill = new(Color.FromRgb(216, 90, 48));
        private static readonly SolidColorBrush StartFill = new(Color.FromRgb(99, 153, 34));
        private static readonly SolidColorBrush ConnLine = new(Color.FromRgb(100, 160, 230));
        private static readonly SolidColorBrush PathLine = new(Color.FromRgb(56, 142, 60));
        private static readonly SolidColorBrush PathNode = new(Color.FromRgb(200, 30, 30));

        // Room fill colors (ciclate pe index)
        private static readonly Color[] RoomColors =
        {
            Color.FromArgb(60,  99, 153, 34),   // verde
            Color.FromArgb(60,  33, 150, 243),  // albastru
            Color.FromArgb(60, 156, 39, 176),   // mov
            Color.FromArgb(60, 255, 152, 0),    // portocaliu
            Color.FromArgb(60, 121, 85, 72),    // maro
        };

        private readonly Canvas _canvas;
        private readonly BuildingPlan _plan;

        // Drag
        private BuildingElement? _dragging;
        private Point _dragOffset;
        private bool _isDragging;

        // Resize
        private BuildingElement? _resizing;
        private string _resizeHandle = "";  // "BR" = bottom-right
        private Point _resizeStart;
        private double _resizeOrigW, _resizeOrigH;

        // Conectare
        private BuildingElement? _connectFrom;

        // Drum optim
        private List<string> _optimalPath = new();

        public bool IsConnectMode { get; set; } = false;

        public event Action? PlanChanged;

        // ─── Constructor ─────────────────────────────────────────────────────────
        public FloorPlanDrawing(Canvas canvas, BuildingPlan plan)
        {
            _canvas = canvas;
            _plan = plan;

            _canvas.Background = new SolidColorBrush(Color.FromRgb(28, 39, 48));
            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseLeftButtonUp += OnCanvasMouseUp;
        }

        // ─── API public ──────────────────────────────────────────────────────────

        public void AddElement(BuildingElementType type, Point pos)
        {
            var el = new BuildingElement(type, pos);
            _plan.AddElement(el);
            Redraw();
            PlanChanged?.Invoke();
        }

        public void Redraw()
        {
            _canvas.Children.Clear();

            // 1. Grila de fundal
            DrawGrid();

            // 2. Conexiuni (sub elemente)
            foreach (var conn in _plan.Connections)
                DrawConnection(conn);

            // 3. Elementele arhitecturale
            int roomIdx = 0;
            foreach (var el in _plan.Elements)
            {
                Color roomColor = el.Type == BuildingElementType.Room
                    ? RoomColors[roomIdx++ % RoomColors.Length]
                    : Colors.Transparent;
                DrawElement(el, roomColor);
            }

            // 4. Ruta optimă deasupra tuturor
            if (_optimalPath.Count >= 2)
                DrawOptimalPath();

            // 5. Legendă
            DrawLegend();
        }

        public void HighlightPath(IEnumerable<string> nodeIds)
        {
            _optimalPath = nodeIds.ToList();
            Redraw();
        }

        public void ClearPath()
        {
            _optimalPath.Clear();
            Redraw();
        }

        // ─── Grid de fundal ───────────────────────────────────────────────────────
        private void DrawGrid()
        {
            double w = _canvas.ActualWidth > 10 ? _canvas.ActualWidth : 1200;
            double h = _canvas.ActualHeight > 10 ? _canvas.ActualHeight : 800;
            const double step = 40;
            var pen = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));

            for (double x = 0; x <= w; x += step)
            {
                var l = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = h,
                    Stroke = pen,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(l);
            }
            for (double y = 0; y <= h; y += step)
            {
                var l = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = w,
                    Y2 = y,
                    Stroke = pen,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(l);
            }
        }

        // ─── Desenare element ─────────────────────────────────────────────────────
        private void DrawElement(BuildingElement el, Color roomColor)
        {
            switch (el.Type)
            {
                case BuildingElementType.Room:
                    DrawRoom(el, roomColor);
                    break;
                case BuildingElementType.ExitDoor:
                    DrawExitDoor(el);
                    break;
                case BuildingElementType.StartPoint:
                    DrawStartPoint(el);
                    break;
                case BuildingElementType.Stairs:
                    DrawStairs(el);
                    break;
                case BuildingElementType.Elevator:
                    DrawElevator(el);
                    break;
            }
        }

        // ─── CAMERĂ ──────────────────────────────────────────────────────────────
        private void DrawRoom(BuildingElement el, Color fillColor)
        {
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;
            bool onPath = _optimalPath.Contains(el.Id);

            // Umplutură cameră
            var fill = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = new SolidColorBrush(fillColor),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(fill, x); Canvas.SetTop(fill, y);
            Canvas.SetZIndex(fill, 1);
            _canvas.Children.Add(fill);

            // Pereți (bordură groasă)
            var border = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = Brushes.Transparent,
                Stroke = onPath
                    ? new SolidColorBrush(Color.FromRgb(200, 30, 30))
                    : WallBrush,
                StrokeThickness = onPath ? WallThick + 2 : WallThick,
                Cursor = Cursors.SizeAll,
                Tag = el.Id
            };
            Canvas.SetLeft(border, x); Canvas.SetTop(border, y);
            Canvas.SetZIndex(border, 10);
            border.MouseLeftButtonDown += (s, e) => OnElementMouseDown(el, e);
            border.MouseRightButtonDown += (s, e) => { ShowContextMenu(el); e.Handled = true; };
            _canvas.Children.Add(border);

            // Etichetă cameră
            var lbl = new TextBlock
            {
                Text = el.Label,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 190)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w - 10
            };
            Canvas.SetLeft(lbl, x + 5);
            Canvas.SetTop(lbl, y + 8);
            Canvas.SetZIndex(lbl, 11);
            _canvas.Children.Add(lbl);

            // ID mic
            var idlbl = new TextBlock
            {
                Text = el.Id,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 200, 200, 180)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w - 10
            };
            Canvas.SetLeft(idlbl, x + 5);
            Canvas.SetTop(idlbl, y + h - 18);
            Canvas.SetZIndex(idlbl, 11);
            _canvas.Children.Add(idlbl);

            // Handle resize (pătrat mic în colțul dreapta-jos)
            DrawResizeHandle(el);
        }

        private void DrawResizeHandle(BuildingElement el)
        {
            double rx = el.Position.X + el.Width - 10;
            double ry = el.Position.Y + el.Height - 10;

            var handle = new Rectangle
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Color.FromArgb(180, 100, 160, 220)),
                Stroke = Brushes.White,
                StrokeThickness = 0.8,
                Cursor = Cursors.SizeNWSE,
                Tag = "resize_" + el.Id
            };
            Canvas.SetLeft(handle, rx); Canvas.SetTop(handle, ry);
            Canvas.SetZIndex(handle, 20);
            handle.MouseLeftButtonDown += (s, e) => OnResizeMouseDown(el, e);
            _canvas.Children.Add(handle);
        }
        // ─── UȘĂ EVACUARE ────────────────────────────────────────────────────────
        private void DrawExitDoor(BuildingElement el)
        {
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;
            bool onPath = _optimalPath.Contains(el.Id);

            // Dreptunghi EXIT
            var box = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = onPath
                    ? new SolidColorBrush(Color.FromRgb(180, 20, 20))
                    : ExitFill,
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                RadiusX = 3,
                RadiusY = 3,
                Cursor = Cursors.Hand,
                Tag = el.Id
            };
            Canvas.SetLeft(box, x); Canvas.SetTop(box, y);
            Canvas.SetZIndex(box, 10);
            box.MouseLeftButtonDown += (s, e) => OnElementMouseDown(el, e);
            box.MouseRightButtonDown += (s, e) => { ShowContextMenu(el); e.Handled = true; };
            _canvas.Children.Add(box);

            // Săgeată la dreapta
            double cx = x + w / 2, cy = y + h / 2;
            DrawArrow(cx - 8, cy, cx + w / 2 + 14, cy, Brushes.White, 2.5);

            // Text EXIT
            var tb = new TextBlock
            {
                Text = "EXIT",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y + h / 2 - 8);
            Canvas.SetZIndex(tb, 11);
            _canvas.Children.Add(tb);

            // ID
            var idtb = new TextBlock
            {
                Text = el.Id,
                FontSize = 8,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 200, 180)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetLeft(idtb, x); Canvas.SetTop(idtb, y + h + 2);
            Canvas.SetZIndex(idtb, 11);
            _canvas.Children.Add(idtb);
        }

        // ─── PUNCT START ──────────────────────────────────────────────────────────
        private void DrawStartPoint(BuildingElement el)
        {
            double cx = el.Position.X + el.Width / 2;
            double cy = el.Position.Y + el.Height / 2;
            double r = Math.Min(el.Width, el.Height) / 2;
            bool onPath = _optimalPath.Contains(el.Id);

            var circle = new Ellipse
            {
                Width = r * 2,
                Height = r * 2,
                Fill = onPath
                    ? new SolidColorBrush(Color.FromRgb(180, 20, 20))
                    : StartFill,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Cursor = Cursors.Hand,
                Tag = el.Id
            };
            Canvas.SetLeft(circle, cx - r); Canvas.SetTop(circle, cy - r);
            Canvas.SetZIndex(circle, 10);
            circle.MouseLeftButtonDown += (s, e) => OnElementMouseDown(el, e);
            circle.MouseRightButtonDown += (s, e) => { ShowContextMenu(el); e.Handled = true; };
            _canvas.Children.Add(circle);

            // Simbol S
            var tb = new TextBlock
            {
                Text = "S",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = r * 2
            };
            Canvas.SetLeft(tb, cx - r); Canvas.SetTop(tb, cy - 10);
            Canvas.SetZIndex(tb, 11);
            _canvas.Children.Add(tb);

            var idtb = new TextBlock
            {
                Text = el.Id,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 220, 100)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = 80
            };
            Canvas.SetLeft(idtb, cx - 40); Canvas.SetTop(idtb, cy + r + 2);
            Canvas.SetZIndex(idtb, 11);
            _canvas.Children.Add(idtb);
        }

        // ─── SCĂRI ────────────────────────────────────────────────────────────────
        private void DrawStairs(BuildingElement el)
        {
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;
            bool onPath = _optimalPath.Contains(el.Id);

            // Fundal
            var bg = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = StairFill,
                Stroke = onPath ? PathNode : WallBrush,
                StrokeThickness = onPath ? 2.5 : 1.5,
                Cursor = Cursors.Hand,
                Tag = el.Id
            };
            Canvas.SetLeft(bg, x); Canvas.SetTop(bg, y);
            Canvas.SetZIndex(bg, 10);
            bg.MouseLeftButtonDown += (s, e) => OnElementMouseDown(el, e);
            bg.MouseRightButtonDown += (s, e) => { ShowContextMenu(el); e.Handled = true; };
            _canvas.Children.Add(bg);

            // Linii trepte (orizontale)
            int nSteps = Math.Max(3, (int)((h - 10) / StairsStep));
            double stepH = (h - 10) / nSteps;
            for (int i = 1; i <= nSteps; i++)
            {
                double sy = y + i * stepH;
                var l = new Line
                {
                    X1 = x + 4,
                    Y1 = sy,
                    X2 = x + w - 4,
                    Y2 = sy,
                    Stroke = StairLine,
                    StrokeThickness = 1,
                    IsHitTestVisible = false
                };
                Canvas.SetZIndex(l, 11);
                _canvas.Children.Add(l);
            }

            // Săgeată verticală (direcție urcare)
            DrawArrow(x + w / 2, y + h - 8, x + w / 2, y + 8,
                      new SolidColorBrush(Color.FromRgb(68, 68, 65)), 1.5, zIndex: 12);

            // Eticheta
            var tb = new TextBlock
            {
                Text = "SCARI",
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(68, 68, 65)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y + h + 2);
            Canvas.SetZIndex(tb, 12);
            _canvas.Children.Add(tb);
        }

        // ─── LIFT ─────────────────────────────────────────────────────────────────
        private void DrawElevator(BuildingElement el)
        {
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;
            bool onPath = _optimalPath.Contains(el.Id);

            var bg = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = ElevFill,
                Stroke = onPath ? PathNode : ElevStroke,
                StrokeThickness = onPath ? 2.5 : 1.5,
                Cursor = Cursors.Hand,
                Tag = el.Id
            };
            Canvas.SetLeft(bg, x); Canvas.SetTop(bg, y);
            Canvas.SetZIndex(bg, 10);
            bg.MouseLeftButtonDown += (s, e) => OnElementMouseDown(el, e);
            bg.MouseRightButtonDown += (s, e) => { ShowContextMenu(el); e.Handled = true; };
            _canvas.Children.Add(bg);

            // X interior
            var d1 = new Line
            {
                X1 = x + 6,
                Y1 = y + 6,
                X2 = x + w - 6,
                Y2 = y + h - 6,
                Stroke = ElevStroke,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            var d2 = new Line
            {
                X1 = x + w - 6,
                Y1 = y + 6,
                X2 = x + 6,
                Y2 = y + h - 6,
                Stroke = ElevStroke,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            Canvas.SetZIndex(d1, 11); Canvas.SetZIndex(d2, 11);
            _canvas.Children.Add(d1); _canvas.Children.Add(d2);

            var tb = new TextBlock
            {
                Text = "LIFT",
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(24, 95, 165)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y + h + 2);
            Canvas.SetZIndex(tb, 12);
            _canvas.Children.Add(tb);
        }

        // ─── CONEXIUNI (holuri) ───────────────────────────────────────────────────
        private void DrawConnection(HallwayConnection conn)
        {
            var fromEl = _plan.Elements.FirstOrDefault(e => e.Id == conn.FromId);
            var toEl = _plan.Elements.FirstOrDefault(e => e.Id == conn.ToId);
            if (fromEl == null || toEl == null) return;

            bool onPath = IsConnOnPath(conn);

            // Traseu în formă de L: nu mai desenăm centru -> centru, ci margine -> cot -> margine.
            List<Point> route = BuildLRouteBetweenElements(fromEl, toEl);
            if (route.Count < 2) return;

            // Grosimea holului este calculată din înălțimea camerei.
            // Dacă ambele sunt camere, folosim înălțimea mai mică, ca să nu iasă disproporționat.
            double hallwayThickness = GetHallwayThickness(fromEl, toEl);

            // Corpul holului. Este desenat sub camere, iar camerele îl acoperă la capete,
            // deci pare că intră exact din marginea camerei.
            DrawPolylinePath(
                route,
                onPath
                    ? new SolidColorBrush(Color.FromArgb(210, 56, 142, 60))
                    : new SolidColorBrush(Color.FromArgb(110, 100, 160, 230)),
                hallwayThickness,
                zIndex: onPath ? 6 : 3,
                dashed: false,
                opacity: onPath ? 0.95 : 0.75);

            // Linie centrală subțire, doar ca să se vadă direcția/conexiunea în mijlocul holului.
            DrawPolylinePath(
                route,
                onPath ? PathLine : ConnLine,
                onPath ? 3.0 : 1.8,
                zIndex: onPath ? 7 : 4,
                dashed: !onPath,
                opacity: 1.0);

            // Etichetă cap/timp pe segmentul din mijloc.
            DrawConnectionLabel(route, $"c:{conn.Capacity} t:{conn.TravelTime}", onPath);
        }

        // ─── RUTA OPTIMĂ ─────────────────────────────────────────────────────────
        private void DrawOptimalPath()
        {
            // Holurile de pe ruta optimă sunt deja colorate în DrawConnection(),
            // deoarece DrawConnection() verifică IsConnOnPath(conn).
            // Aici desenăm doar săgeți și puncte mici peste elemente, ca să nu acoperim camerele
            // cu un hol verde foarte gros.
            var elements = new List<BuildingElement>();
            foreach (var id in _optimalPath)
            {
                var el = _plan.Elements.FirstOrDefault(e => e.Id == id);
                if (el != null) elements.Add(el);
            }
            if (elements.Count < 2) return;

            // Săgeți pe fiecare segment al traseului în L.
            for (int i = 0; i < elements.Count - 1; i++)
            {
                List<Point> route = BuildLRouteBetweenElements(elements[i], elements[i + 1]);
                DrawArrowOnRoute(route, PathLine, zIndex: 51);
            }

            // Cercuri pe noduri.
            foreach (var el in elements)
            {
                Point p = el.Center;
                var c = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = PathLine,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(c, p.X - 6); Canvas.SetTop(c, p.Y - 6);
                Canvas.SetZIndex(c, 52);
                _canvas.Children.Add(c);
            }
        }

        // ─── LEGENDĂ ─────────────────────────────────────────────────────────────
        private void DrawLegend()
        {
            double cw = _canvas.ActualWidth > 10 ? _canvas.ActualWidth : 1200;
            double ch = _canvas.ActualHeight > 10 ? _canvas.ActualHeight : 800;
            double lx = cw - 180, ly = ch - 160;

            var bg = new Rectangle
            {
                Width = 170,
                Height = 150,
                Fill = new SolidColorBrush(Color.FromArgb(200, 20, 30, 40)),
                Stroke = new SolidColorBrush(Color.FromArgb(80, 100, 150, 200)),
                StrokeThickness = 0.8,
                RadiusX = 4,
                RadiusY = 4,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(bg, lx); Canvas.SetTop(bg, ly);
            Canvas.SetZIndex(bg, 90);
            _canvas.Children.Add(bg);

            var items = new[]
            {
                ("■", Color.FromRgb(99, 153, 34),  "Camera"),
                ("∩", Color.FromRgb(44, 44, 42),   "Usa"),
                ("≡", Color.FromRgb(136,135,128),   "Scari"),
                ("✕", Color.FromRgb(55, 138, 221),  "Lift"),
                ("►", Color.FromRgb(216, 90, 48),   "Iesire"),
                ("●", Color.FromRgb(99, 153, 34),   "Start"),
                ("─", Color.FromRgb(56, 142, 60),   "Ruta optima"),
            };

            for (int i = 0; i < items.Length; i++)
            {
                var (sym, col, text) = items[i];
                double iy = ly + 14 + i * 19;

                var stb = new TextBlock
                {
                    Text = sym,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(col),
                    IsHitTestVisible = false,
                    Width = 18,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(stb, lx + 8); Canvas.SetTop(stb, iy);
                Canvas.SetZIndex(stb, 91);
                _canvas.Children.Add(stb);

                var ttb = new TextBlock
                {
                    Text = text,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 210, 220)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(ttb, lx + 30); Canvas.SetTop(ttb, iy + 1);
                Canvas.SetZIndex(ttb, 91);
                _canvas.Children.Add(ttb);
            }
        }

        // ─── Helpers pentru holuri în formă de L ──────────────────────────────────────
        private List<Point> BuildLRouteBetweenElements(BuildingElement fromEl, BuildingElement toEl)
        {
            Point fromCenter = fromEl.Center;
            Point toCenter = toEl.Center;

            Point start = GetBorderPointToward(fromEl, toCenter);
            Point end = GetBorderPointToward(toEl, fromCenter);

            return BuildLRoute(start, end);
        }

        private List<Point> BuildLRoute(Point start, Point end)
        {
            var points = new List<Point> { start };

            double dx = Math.Abs(end.X - start.X);
            double dy = Math.Abs(end.Y - start.Y);

            // Dacă sunt deja pe aceeași orizontală sau verticală, desenăm direct.
            if (dx < 0.001 || dy < 0.001)
            {
                points.Add(end);
                return points;
            }

            // Traseu simplu în formă de L: start -> un singur cot -> end.
            // Dacă distanța pe X este mai mare, mergem întâi orizontal, apoi vertical.
            // Dacă distanța pe Y este mai mare, mergem întâi vertical, apoi orizontal.
            Point corner = dx >= dy
                ? new Point(end.X, start.Y)   // orizontal, apoi vertical
                : new Point(start.X, end.Y);  // vertical, apoi orizontal

            points.Add(corner);
            points.Add(end);
            return points;
        }

        private Point GetBorderPointToward(BuildingElement el, Point target)
        {
            double x = el.Position.X;
            double y = el.Position.Y;
            double w = el.Width;
            double h = el.Height;

            Point center = el.Center;
            double dx = target.X - center.X;
            double dy = target.Y - center.Y;

            // Pentru cercul de Start, punctul de ieșire este pe circumferință, nu pe bounding box.
            if (el.Type == BuildingElementType.StartPoint)
            {
                double len = Math.Sqrt(dx * dx + dy * dy);
                if (len < 0.001) return center;

                double r = Math.Min(el.Width, el.Height) / 2.0;
                return new Point(center.X + dx / len * r, center.Y + dy / len * r);
            }

            // Pentru restul elementelor, alegem latura spre care este celălalt element.
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                // Ieșire prin stânga/dreapta.
                return dx >= 0
                    ? new Point(x + w, center.Y)
                    : new Point(x, center.Y);
            }
            else
            {
                // Ieșire prin sus/jos.
                return dy >= 0
                    ? new Point(center.X, y + h)
                    : new Point(center.X, y);
            }
        }

        private double GetHallwayThickness(BuildingElement fromEl, BuildingElement toEl)
        {
            // Cerința ta: holul să fie de înălțimea camerei.
            // Dacă legătura este între două camere, folosim înălțimea celei mai mici camere.
            if (fromEl.Type == BuildingElementType.Room && toEl.Type == BuildingElementType.Room)
                return Math.Min(fromEl.Height, toEl.Height);

            if (fromEl.Type == BuildingElementType.Room)
                return fromEl.Height;

            if (toEl.Type == BuildingElementType.Room)
                return toEl.Height;

            // Pentru Start, Exit, Scări, Lift etc., folosim o grosime rezonabilă.
            return 48;
        }

        private void DrawPolylinePath(List<Point> points, Brush brush, double thickness,
                                      int zIndex, bool dashed = false, double opacity = 1.0)
        {
            if (points == null || points.Count < 2) return;

            var figure = new PathFigure
            {
                StartPoint = points[0],
                IsClosed = false,
                IsFilled = false
            };

            var segment = new PolyLineSegment();
            for (int i = 1; i < points.Count; i++)
                segment.Points.Add(points[i]);

            figure.Segments.Add(segment);

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            var path = new Path
            {
                Data = geometry,
                Stroke = brush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Flat,
                StrokeLineJoin = PenLineJoin.Miter,
                Opacity = opacity,
                IsHitTestVisible = false
            };

            if (dashed)
                path.StrokeDashArray = new DoubleCollection { 8, 4 };

            Canvas.SetZIndex(path, zIndex);
            _canvas.Children.Add(path);
        }

        private void DrawConnectionLabel(List<Point> route, string text, bool onPath)
        {
            if (route == null || route.Count < 2) return;

            int segIndex = Math.Max(0, (route.Count - 1) / 2);
            Point a = route[segIndex];
            Point b = route[segIndex + 1];

            double mx = (a.X + b.X) / 2.0 + 4;
            double my = (a.Y + b.Y) / 2.0 - 10;

            var lbl = new TextBlock
            {
                Text = text,
                FontSize = 9,
                Foreground = onPath
                    ? PathLine
                    : new SolidColorBrush(Color.FromArgb(200, 210, 230, 255)),
                Background = new SolidColorBrush(Color.FromArgb(120, 28, 39, 48)),
                Padding = new Thickness(2, 0, 2, 0),
                IsHitTestVisible = false
            };

            Canvas.SetLeft(lbl, mx);
            Canvas.SetTop(lbl, my);
            Canvas.SetZIndex(lbl, 8);
            _canvas.Children.Add(lbl);
        }

        private void DrawArrowOnRoute(List<Point> route, Brush brush, int zIndex)
        {
            if (route == null || route.Count < 2) return;

            // Punem săgeata pe ultimul segment suficient de lung.
            for (int i = route.Count - 2; i >= 0; i--)
            {
                Point a = route[i];
                Point b = route[i + 1];
                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double len = Math.Sqrt(dx * dx + dy * dy);

                if (len < 12) continue;

                double ux = dx / len;
                double uy = dy / len;

                Point p2 = new Point(a.X + dx * 0.65, a.Y + dy * 0.65);
                Point p1 = new Point(p2.X - ux * 22, p2.Y - uy * 22);

                DrawArrow(p1.X, p1.Y, p2.X, p2.Y, brush, 3, zIndex: zIndex, arrowOnly: false);
                return;
            }
        }

        // ─── Helpers geometrie ────────────────────────────────────────────────────
        private void DrawArrow(double x1, double y1, double x2, double y2,
                               Brush brush, double thick,
                               int zIndex = 12, bool arrowOnly = false)
        {
            double dx = x2 - x1, dy = y2 - y1;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1) return;

            double ux = dx / len, uy = dy / len;
            double arrowLen = 10, arrowW = 5;

            // Punctul de vârf
            double ax = x2 - ux * arrowLen;
            double ay = y2 - uy * arrowLen;

            // Punctele aripilor
            double wx1 = ax - uy * arrowW, wy1 = ay + ux * arrowW;
            double wx2 = ax + uy * arrowW, wy2 = ay - ux * arrowW;

            if (!arrowOnly)
            {
                var line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = brush,
                    StrokeThickness = thick,
                    IsHitTestVisible = false
                };
                Canvas.SetZIndex(line, zIndex);
                _canvas.Children.Add(line);
            }

            // Triunghi săgeată
            var poly = new Polygon
            {
                Points = new PointCollection { new(x2, y2), new(wx1, wy1), new(wx2, wy2) },
                Fill = brush,
                IsHitTestVisible = false
            };
            Canvas.SetZIndex(poly, zIndex + 1);
            _canvas.Children.Add(poly);
        }

        private bool IsConnOnPath(HallwayConnection conn)
        {
            if (_optimalPath.Count < 2) return false;
            for (int i = 0; i < _optimalPath.Count - 1; i++)
            {
                if ((_optimalPath[i] == conn.FromId && _optimalPath[i + 1] == conn.ToId) ||
                    (conn.IsBidirectional &&
                     _optimalPath[i] == conn.ToId && _optimalPath[i + 1] == conn.FromId))
                    return true;
            }
            return false;
        }

        // ─── Mouse events ─────────────────────────────────────────────────────────
        private void OnElementMouseDown(BuildingElement el, MouseButtonEventArgs e)
        {
            if (IsConnectMode)
            {
                if (_connectFrom == null)
                {
                    _connectFrom = el;
                    Redraw();
                    DrawSelectionIndicator(el);
                }
                else if (_connectFrom.Id != el.Id)
                {
                    _plan.AddConnection(new HallwayConnection(_connectFrom.Id, el.Id));
                    _connectFrom = null;
                    Redraw();
                    PlanChanged?.Invoke();
                }
                else
                {
                    _connectFrom = null;
                    Redraw();
                }
                e.Handled = true;
                return;
            }

            _dragging = el;
            _isDragging = false;
            _dragOffset = e.GetPosition(_canvas) - new Vector(el.Position.X, el.Position.Y);
            _canvas.CaptureMouse();
            e.Handled = true;
        }

        private void OnResizeMouseDown(BuildingElement el, MouseButtonEventArgs e)
        {
            _resizing = el;
            _resizeHandle = "BR";
            _resizeStart = e.GetPosition(_canvas);
            _resizeOrigW = el.Width;
            _resizeOrigH = el.Height;
            _canvas.CaptureMouse();
            e.Handled = true;
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            Point mp = e.GetPosition(_canvas);

            if (_resizing != null && e.LeftButton == MouseButtonState.Pressed)
            {
                double dw = mp.X - _resizeStart.X;
                double dh = mp.Y - _resizeStart.Y;
                _resizing.Width = Math.Max(60, _resizeOrigW + dw);
                _resizing.Height = Math.Max(40, _resizeOrigH + dh);
                Redraw();
                return;
            }

            if (_dragging != null && e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragging.Position = new Point(
                    mp.X - _dragOffset.X,
                    mp.Y - _dragOffset.Y);
                Redraw();
            }
        }

        private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_resizing != null)
            {
                _canvas.ReleaseMouseCapture();
                _resizing = null;
                PlanChanged?.Invoke();
                return;
            }

            if (_dragging != null)
            {
                _canvas.ReleaseMouseCapture();
                if (_isDragging) PlanChanged?.Invoke();
                _dragging = null;
                _isDragging = false;
            }
        }

        private void DrawSelectionIndicator(BuildingElement el)
        {
            double x = el.Position.X - 4, y = el.Position.Y - 4;
            double w = el.Width + 8, h = el.Height + 8;

            if (el.Type == BuildingElementType.StartPoint)
            {
                x = el.Center.X - el.Width / 2 - 8;
                y = el.Center.Y - el.Height / 2 - 8;
                w = h = el.Width + 16;
            }

            var sel = new Rectangle
            {
                Width = w,
                Height = h,
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                Fill = Brushes.Transparent,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(sel, x); Canvas.SetTop(sel, y);
            Canvas.SetZIndex(sel, 80);
            _canvas.Children.Add(sel);
        }

        // ─── Context menu ─────────────────────────────────────────────────────────
        private void ShowContextMenu(BuildingElement el)
        {
            var menu = new ContextMenu();

            var header = new MenuItem
            {
                Header = $"{el.Type}: {el.Id}  (cap:{el.Capacity} t:{el.TravelTime})",
                IsEnabled = false,
                FontWeight = FontWeights.Bold
            };
            menu.Items.Add(header);
            menu.Items.Add(new Separator());

            if (el.Type == BuildingElementType.Room)
            {
                var editLabel = new MenuItem { Header = "Redenumește camera..." };
                editLabel.Click += (s, e) =>
                {
                    var dlg = new RenameDialog(el.Label);
                    if (dlg.ShowDialog() == true)
                    { el.Label = dlg.NewName; Redraw(); PlanChanged?.Invoke(); }
                };
                menu.Items.Add(editLabel);
                menu.Items.Add(new Separator());
            }

            // Editare capacitate conexiuni
            var conns = _plan.Connections.Where(c => c.FromId == el.Id || c.ToId == el.Id).ToList();
            if (conns.Count > 0)
            {
                var connMenu = new MenuItem { Header = $"Conexiuni ({conns.Count})" };
                foreach (var conn in conns)
                {
                    string other = conn.FromId == el.Id ? conn.ToId : conn.FromId;
                    var sub = new MenuItem { Header = $"↔ {other}  cap:{conn.Capacity} t:{conn.TravelTime}" };

                    Action redrawConn = () => { Redraw(); PlanChanged?.Invoke(); };
                    AddCapTimeMenuItems(sub, conn, redrawConn);

                    var del = new MenuItem { Header = "Șterge conexiunea" };
                    del.Click += (s, e) => { _plan.RemoveConnection(conn); Redraw(); PlanChanged?.Invoke(); };
                    sub.Items.Add(new Separator());
                    sub.Items.Add(del);
                    connMenu.Items.Add(sub);
                }
                menu.Items.Add(connMenu);
                menu.Items.Add(new Separator());
            }

            var deleteItem = new MenuItem { Header = "🗑  Șterge elementul" };
            deleteItem.Click += (s, e) => { _plan.RemoveElement(el); Redraw(); PlanChanged?.Invoke(); };
            menu.Items.Add(deleteItem);

            _canvas.ContextMenu = menu;
            menu.IsOpen = true;
        }

        private static void AddCapTimeMenuItems(MenuItem parent, HallwayConnection conn, Action refresh)
        {
            void Add(string h, Action a)
            { var m = new MenuItem { Header = h }; m.Click += (s, e) => { a(); refresh(); }; parent.Items.Add(m); }

            Add("Capacitate  +1", () => conn.Capacity++);
            Add("Capacitate  −1", () => { if (conn.Capacity > 1) conn.Capacity--; });
            parent.Items.Add(new Separator());
            Add("TravelTime +1", () => conn.TravelTime++);
            Add("TravelTime −1", () => { if (conn.TravelTime > 1) conn.TravelTime--; });
        }
    }

    // ─── Dialog redenumire ────────────────────────────────────────────────────────
    internal class RenameDialog : System.Windows.Window
    {
        private readonly System.Windows.Controls.TextBox _tb;
        public string NewName => _tb.Text;

        public RenameDialog(string current)
        {
            Title = "Redenumește";
            Width = 300; Height = 130;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(32, 42, 52));

            var sp = new StackPanel { Margin = new Thickness(16) };

            _tb = new System.Windows.Controls.TextBox
            {
                Text = current,
                Height = 28,
                Background = new SolidColorBrush(Color.FromRgb(20, 28, 38)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 80, 100)),
                Margin = new Thickness(0, 0, 0, 10)
            };
            _tb.SelectAll();

            var btn = new Button
            {
                Content = "OK",
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(0, 105, 92)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            btn.Click += (s, e) => { DialogResult = true; };
            _tb.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Return) { DialogResult = true; } };

            sp.Children.Add(_tb);
            sp.Children.Add(btn);
            Content = sp;
            Loaded += (s, e) => _tb.Focus();
        }
    }
}