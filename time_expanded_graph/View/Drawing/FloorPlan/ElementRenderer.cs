using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Drawing.FloorPlan.Elements
{
    public class ElementRenderer
    {
        private readonly Canvas _canvas;
        private readonly RoomRenderer _roomRenderer;
        private readonly ExitDoorRenderer _exitRenderer;
        private readonly StartPointRenderer _startRenderer;
        private readonly StairsRenderer _stairsRenderer;
        private readonly ElevatorRenderer _elevatorRenderer;

        public event Action<BuildingElement, MouseButtonEventArgs>? ElementMouseDown;
        public event Action<BuildingElement, MouseButtonEventArgs>? ContextMenuRequested;
        public event Action<BuildingElement, MouseButtonEventArgs>? ResizeMouseDown;

        public ElementRenderer(Canvas canvas)
        {
            _canvas = canvas;
            _roomRenderer = new RoomRenderer(canvas);
            _exitRenderer = new ExitDoorRenderer(canvas);
            _startRenderer = new StartPointRenderer(canvas);
            _stairsRenderer = new StairsRenderer(canvas);
            _elevatorRenderer = new ElevatorRenderer(canvas);

            WireCommonEvents(_roomRenderer);
            WireCommonEvents(_exitRenderer);
            WireCommonEvents(_startRenderer);
            WireCommonEvents(_stairsRenderer);
            WireCommonEvents(_elevatorRenderer);

            _roomRenderer.ResizeMouseDown += (el, e) => ResizeMouseDown?.Invoke(el, e);
        }
        private void WireCommonEvents(dynamic renderer)
        {
            renderer.ElementMouseDown += (Action<BuildingElement, MouseButtonEventArgs>)((el, e) =>
                ElementMouseDown?.Invoke(el, e));
            renderer.ContextMenuRequested += (Action<BuildingElement, MouseButtonEventArgs>)((el, e) =>
                ContextMenuRequested?.Invoke(el, e));
        }
        public void DrawAll(List<BuildingElement> elements, List<string> optimalPath)
        {
            int roomIdx = 0;
            foreach (var el in elements)
            {
                bool onPath = optimalPath.Contains(el.Id);

                switch (el.Type)
                {
                    case BuildingElementType.Room:
                        _roomRenderer.Draw(el, roomIdx++, onPath);
                        break;
                    case BuildingElementType.ExitDoor:
                        _exitRenderer.Draw(el, onPath);
                        break;
                    case BuildingElementType.StartPoint:
                        _startRenderer.Draw(el, onPath);
                        break;
                    case BuildingElementType.Stairs:
                        _stairsRenderer.Draw(el, onPath);
                        break;
                    case BuildingElementType.Elevator:
                        _elevatorRenderer.Draw(el, onPath);
                        break;
                }
            }
        }
    }
}