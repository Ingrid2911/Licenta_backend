public enum ExpandedEdgeType
{
    Holdover,   // a_2 -> a_3 (așteptare)
    Movement    // a_2 -> b_3 (deplasare)
}
namespace time_expanded_graph.Models.Graphs
{
   
    public class ExpandedEdge
    {
        public string From { get; private set; } //conventie: nod_original + "_" + timp
        public string To { get; private set; }
        public int Capacity { get; private set; }
        public ExpandedEdgeType Type { get; private set; } 
        public ExpandedEdge(string from, string to, int capacity, ExpandedEdgeType type)
        {
            From = from;
            To = to;
            Capacity = capacity;
            Type = type;
        }

    }
}
