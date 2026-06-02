using System.Windows;

namespace time_expanded_graph.Models.Building
{
    public class BuildingElement
    {
        private static int _idCounter = 0;

        public string Id { get; private set; }
        public BuildingElementType Type { get; set; }

        // Geometrie pe canvas
        public Point Position { get; set; }   // colt stanga-sus
        public double Width { get; set; }
        public double Height { get; set; }
        public string Label { get; set; }

        // Proprietati graf
        public int Capacity { get; set; }
        public int TravelTime { get; set; }

        // Centrul elementului (pentru linii conexiune)
        public Point Center => new Point(Position.X + Width / 2, Position.Y + Height / 2);

        public bool IsNode => true; // toate elementele arhitecturale sunt noduri

        public BuildingElement(BuildingElementType type, Point position,
                               double width = 0, double height = 0)
        {
            _idCounter++;
            Id = $"{TypePrefix(type)}_{_idCounter}";
            Type = type;
            Position = position;
            Width = width > 0 ? width : DefaultWidth(type);
            Height = height > 0 ? height : DefaultHeight(type);
            Label = DefaultLabel(type, _idCounter);
            Capacity = DefaultCapacity(type);
            TravelTime = DefaultTravelTime(type);
        }

        public static void ResetCounter() => _idCounter = 0;

        private static string TypePrefix(BuildingElementType t) => t switch
        {
            BuildingElementType.Room => "CAM",
            BuildingElementType.Stairs => "SC",
            BuildingElementType.Elevator => "LF",
            BuildingElementType.ExitDoor => "EXIT",
            BuildingElementType.StartPoint => "S",
            _ => "X"
        };

        private static string DefaultLabel(BuildingElementType t, int n) => t switch
        {
            BuildingElementType.Room => $"Camera {n}",
            BuildingElementType.Stairs => "Scari",
            BuildingElementType.Elevator => "Lift",
            BuildingElementType.ExitDoor => "Iesire",
            BuildingElementType.StartPoint => "Start",
            _ => $"El {n}"
        };

        private static double DefaultWidth(BuildingElementType t) => t switch
        {
            BuildingElementType.Room => 160,
            BuildingElementType.Stairs => 80,
            BuildingElementType.Elevator => 60,
            BuildingElementType.ExitDoor => 48,
            BuildingElementType.StartPoint => 48,
            _ => 60
        };

        private static double DefaultHeight(BuildingElementType t) => t switch
        {
            BuildingElementType.Room => 120,
            BuildingElementType.Stairs => 100,
            BuildingElementType.Elevator => 60,
            BuildingElementType.ExitDoor => 48,
            BuildingElementType.StartPoint => 48,
            _ => 60
        };

        private static int DefaultCapacity(BuildingElementType t) => t switch
        {
            BuildingElementType.Stairs => 2,
            BuildingElementType.Elevator => 4,
            BuildingElementType.ExitDoor => 10,
            _ => 5
        };

        private static int DefaultTravelTime(BuildingElementType t) => t switch
        {
            BuildingElementType.Stairs => 2,
            BuildingElementType.Elevator => 3,
            _ => 1
        };

        public override string ToString() => $"{Id} ({Label})";
    }
}