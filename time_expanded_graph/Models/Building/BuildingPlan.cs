using System.Windows;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.Models.Building
{
    public class BuildingPlan
    {
        public List<BuildingElement> Elements { get; } = new();
        public List<HallwayConnection> Connections { get; } = new();
        public void AddElement(BuildingElement el) => Elements.Add(el);
        public void AddConnection(HallwayConnection c) => Connections.Add(c);

        public void RemoveElement(BuildingElement el)
        {
            Elements.Remove(el);
            Connections.RemoveAll(c => c.FromId == el.Id || c.ToId == el.Id);
        }

        public void RemoveConnection(HallwayConnection c) => Connections.Remove(c);

        public void Clear()
        {
            Elements.Clear();
            Connections.Clear();
            BuildingElement.ResetCounter();
        }

        public (SimpleGraph? graph, string? error) ToSimpleGraph()
        {
            var starts = Elements.Where(e => e.Type == BuildingElementType.StartPoint).ToList();
            var exits = Elements.Where(e => e.Type == BuildingElementType.ExitDoor).ToList();

            if (starts.Count == 0)
                return (null, "Planul nu contine niciun punct de start (S).");
            if (exits.Count == 0)
                return (null, "Planul nu contine nicio usa de evacuare (EXIT).");
            if (Connections.Count == 0)
                return (null, "Planul nu contine conexiuni intre elemente.");

            var nodeIds = Elements.Select(e => e.Id).ToList();
            var edges = new List<SimpleEdge>();

            string sourceId, sinkId;

            if (starts.Count == 1)
            {
                sourceId = starts[0].Id;
            }
            else
            {
                sourceId = "S_virtual";
                nodeIds.Add(sourceId);
                foreach (var s in starts)
                    edges.Add(new SimpleEdge(sourceId, s.Id, 1, 1000));
            }

            if (exits.Count == 1)
            {
                sinkId = exits[0].Id;
            }
            else
            {
                sinkId = "T_virtual";
                nodeIds.Add(sinkId);
                foreach (var ex in exits)
                    edges.Add(new SimpleEdge(ex.Id, sinkId, 1, 1000));
            }

            foreach (var conn in Connections)
            {
                edges.Add(new SimpleEdge(conn.FromId, conn.ToId,
                    conn.TravelTime, conn.Capacity));
                if (conn.IsBidirectional)
                    edges.Add(new SimpleEdge(conn.ToId, conn.FromId,
                        conn.TravelTime, conn.Capacity));
            }

            return (new SimpleGraph(nodeIds, edges, sourceId, sinkId), null);
        }

        public Point? GetPosition(string nodeId)
        {
            var el = Elements.FirstOrDefault(e => e.Id == nodeId);
            return el?.Center;
        }
    }
    public class HallwayConnection
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
        public int Capacity { get; set; } = 5;
        public int TravelTime { get; set; } = 1;
        public bool IsBidirectional { get; set; } = true;
        public HallwayConnection(string fromId, string toId,
                                 int cap = 5, int travel = 1, bool bidir = true)
        {
            FromId = fromId; ToId = toId;
            Capacity = cap; TravelTime = travel; IsBidirectional = bidir;
        }
    }
}