
internal enum ExpandedEdgeType
{
    Holdover,   // a_2 -> a_3 (așteptare)
    Movement    // a_2 -> b_3 (deplasare)
}
namespace time_expanded_graph.ExpandedTimeGraph
{
   
    internal class ExpandedEdge
    {
        //properties
        public string From { get; private set; } //conventie: nod_original + "_" + timp
        public string To { get; private set; }
        public int Capacity { get; private set; }

        //nu mai avem timp, el este deja in nume ex: a_3
        public ExpandedEdgeType Type { get; private set; } // 🔥 IMPORTANT

        public ExpandedEdge(string from, string to, int capacity, ExpandedEdgeType type)
        {
            From = from;
            To = to;
            Capacity = capacity;
            Type = type;
        }

    }
}
