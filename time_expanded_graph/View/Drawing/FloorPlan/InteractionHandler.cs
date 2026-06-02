using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;
using time_expanded_graph.View.Dialogs;

namespace time_expanded_graph.View.Drawing.FloorPlan.Interaction
{
    public class InteractionHandler
    {
        private readonly Canvas _canvas;
        private readonly BuildingPlan _plan;

        private BuildingElement? _dragging;
        private Point _dragOffset;
        private bool _isDragging;

        private BuildingElement? _resizing;
        private Point _resizeStart;
        private double _resizeOrigW, _resizeOrigH;

        private BuildingElement? _connectFrom;

        public bool IsConnectMode { get; set; } = false;

        public event Action? PlanChanged;
        public event Action? RedrawRequested;

        public InteractionHandler(Canvas canvas, BuildingPlan plan)
        {
            _canvas = canvas;
            _plan = plan;

            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseLeftButtonUp += OnCanvasMouseUp;
        }
        public void HandleElementMouseDown(BuildingElement el, MouseButtonEventArgs e)
        {
            if (IsConnectMode)
            {
                HandleConnectMode(el, e);
                return;
            }

            _dragging = el;
            _isDragging = false;
            _dragOffset = e.GetPosition(_canvas) - new Vector(el.Position.X, el.Position.Y);
            _canvas.CaptureMouse();
            e.Handled = true;
        }
        private void HandleConnectMode(BuildingElement el, MouseButtonEventArgs e)
        {
            if (_connectFrom == null)
            {
                _connectFrom = el;
                RedrawRequested?.Invoke();
                DrawSelectionIndicator(el);
            }
            else if (_connectFrom.Id != el.Id)
            {
                var dialog = new HallwaySettingsDialog
                {
                    Owner = Window.GetWindow(_canvas)
                };

                if (dialog.ShowDialog() == true)
                {
                    _plan.AddConnection(new HallwayConnection(
                        _connectFrom.Id, el.Id, dialog.Capacity, dialog.TravelTime, true));
                    PlanChanged?.Invoke();
                }
                _connectFrom = null;
                RedrawRequested?.Invoke();
                PlanChanged?.Invoke();
            }
            else
            {
                _connectFrom = null;
                RedrawRequested?.Invoke();
            }
            e.Handled = true;
        }
        public void HandleResizeMouseDown(BuildingElement el, MouseButtonEventArgs e)
        {
            _dragging = null;

            _resizing = el;
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
                RedrawRequested?.Invoke();
                return;
            }

            if (_dragging != null && e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragging.Position = new Point(
                    mp.X - _dragOffset.X,
                    mp.Y - _dragOffset.Y);
                RedrawRequested?.Invoke();
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
            Panel.SetZIndex(sel, 80);
            _canvas.Children.Add(sel);
        }
    }
}