using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using time_expanded_graph.Models.Building;
using time_expanded_graph.View.Drawing.FloorPlan.Elements;
using time_expanded_graph.View.Drawing.FloorPlan.Grid;
using time_expanded_graph.View.Drawing.FloorPlan.Connections;
using time_expanded_graph.View.Drawing.FloorPlan.Interaction;
using time_expanded_graph.View.Drawing.FloorPlan.OptimalPath;
using time_expanded_graph.View.Drawing.FloorPlan.Legend;

namespace time_expanded_graph.View.Drawing.FloorPlan
{
    public class FloorPlanDrawing
    {
        private readonly Canvas _canvas;
        private readonly BuildingPlan _plan;

        private readonly GridRenderer _gridRenderer;
        private readonly ElementRenderer _elementRenderer;
        private readonly ConnectionRenderer _connectionRenderer;
        private readonly OptimalPathRenderer _pathRenderer;
        private readonly LegendRenderer _legendRenderer;
        private readonly InteractionHandler _interactionHandler;
        private readonly ContextMenuHandler _contextMenuHandler;

        private List<string> _optimalPath = new();

        public bool IsConnectMode
        {
            get => _interactionHandler.IsConnectMode;
            set => _interactionHandler.IsConnectMode = value;
        }

        public event Action? PlanChanged;

        public FloorPlanDrawing(Canvas canvas, BuildingPlan plan)
        {
            _canvas = canvas;
            _plan = plan;

            _canvas.Background = new SolidColorBrush(Color.FromRgb(28, 39, 48));

            _gridRenderer = new GridRenderer(_canvas);
            _elementRenderer = new ElementRenderer(_canvas);
            _connectionRenderer = new ConnectionRenderer(_canvas, _plan);
            _pathRenderer = new OptimalPathRenderer(_canvas, _plan);
            _legendRenderer = new LegendRenderer(_canvas);
            _interactionHandler = new InteractionHandler(_canvas, _plan);
            _contextMenuHandler = new ContextMenuHandler(_plan);

            _interactionHandler.PlanChanged += () => PlanChanged?.Invoke();
            _interactionHandler.RedrawRequested += Redraw;

            _contextMenuHandler.PlanChanged += () => PlanChanged?.Invoke();
            _contextMenuHandler.RedrawRequested += Redraw;

            _elementRenderer.ElementMouseDown += _interactionHandler.HandleElementMouseDown;
            _elementRenderer.ResizeMouseDown += _interactionHandler.HandleResizeMouseDown;
            _elementRenderer.ContextMenuRequested += (el, e) =>
                _contextMenuHandler.ShowContextMenu(el, _canvas);
        }
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

            _gridRenderer.Draw();
            _connectionRenderer.DrawAll(_optimalPath);
            _elementRenderer.DrawAll(_plan.Elements, _optimalPath);

            if (_optimalPath.Count >= 2)
                _pathRenderer.Draw(_optimalPath);

            _legendRenderer.Draw();
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
    }
}