using System.Collections.Generic;

namespace time_expanded_graph.Models.Building
{
    public class BuildingPlanDto
    {
        public int Version { get; set; } = 1;

        public List<BuildingElementDto> Elements { get; set; } = new();

        public List<HallwayConnectionDto> Connections { get; set; } = new();
    }

    public class BuildingElementDto
    {
        public string Id { get; set; } = string.Empty;

        public BuildingElementType Type { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public string Label { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int TravelTime { get; set; }
    }

    public class HallwayConnectionDto
    {
        public string FromId { get; set; } = string.Empty;

        public string ToId { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int TravelTime { get; set; }

        public bool IsBidirectional { get; set; }
    }
}