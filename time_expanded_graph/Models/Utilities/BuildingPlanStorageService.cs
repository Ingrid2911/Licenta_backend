using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.Services
{
    public static class BuildingPlanStorageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public static void Save(BuildingPlan plan, string path)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Calea fișierului nu este validă.", nameof(path));

            var dto = ToDto(plan);

            string json = JsonSerializer.Serialize(dto, JsonOptions);

            File.WriteAllText(path, json);
        }

        public static BuildingPlan Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Calea fișierului nu este validă.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("Fișierul selectat nu există.", path);

            string json = File.ReadAllText(path);

            var dto = JsonSerializer.Deserialize<BuildingPlanDto>(json, JsonOptions);

            if (dto == null)
                throw new InvalidDataException("Fișierul nu conține un plan valid.");

            return FromDto(dto);
        }

        private static BuildingPlanDto ToDto(BuildingPlan plan)
        {
            return new BuildingPlanDto
            {
                Version = 1,

                Elements = plan.Elements.Select(e => new BuildingElementDto
                {
                    Id = e.Id,
                    Type = e.Type,
                    X = e.Position.X,
                    Y = e.Position.Y,
                    Width = e.Width,
                    Height = e.Height,
                    Label = e.Label,
                    Capacity = e.Capacity,
                    TravelTime = e.TravelTime
                }).ToList(),

                Connections = plan.Connections.Select(c => new HallwayConnectionDto
                {
                    FromId = c.FromId,
                    ToId = c.ToId,
                    Capacity = c.Capacity,
                    TravelTime = c.TravelTime,
                    IsBidirectional = c.IsBidirectional
                }).ToList()
            };
        }

        private static BuildingPlan FromDto(BuildingPlanDto dto)
        {
            var plan = new BuildingPlan();

            foreach (var e in dto.Elements)
            {
                var element = new BuildingElement(
                    e.Type,
                    new Point(e.X, e.Y),
                    e.Width,
                    e.Height,
                    e.Id);

                element.Label = string.IsNullOrWhiteSpace(e.Label)
                    ? e.Id
                    : e.Label;

                element.Capacity = e.Capacity;
                element.TravelTime = e.TravelTime;

                plan.AddElement(element);
            }

            var existingIds = plan.Elements
                .Select(e => e.Id)
                .ToHashSet();

            foreach (var c in dto.Connections)
            {
                if (!existingIds.Contains(c.FromId) || !existingIds.Contains(c.ToId))
                    continue;

                plan.AddConnection(new HallwayConnection(
                    c.FromId,
                    c.ToId,
                    c.Capacity,
                    c.TravelTime,
                    c.IsBidirectional));
            }

            return plan;
        }
    }
}
