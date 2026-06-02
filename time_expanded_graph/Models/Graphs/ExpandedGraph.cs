namespace time_expanded_graph.Models.Graphs
{
    public class ExpandedGraph
    {
        //properties
        public HashSet<string> ExpandedNodes { get; private set; } //mai potrivit decat List, impiedica adaugarea automata de noduri duplicate
        public List<ExpandedEdge> ExpandedEdges { get; private set; }
        public string SuperSource { get; private set; } //ex:S*
        public string SuperSink { get; private set; }
        public int TimeHorizon
        {
            get; private set;
        }
        //methods
        public ExpandedGraph(HashSet<string> expandedNodes, List<ExpandedEdge> expandedEdges, string superSource, string superSink, int timeHorizon ) {
            ExpandedNodes = expandedNodes;
            ExpandedEdges = expandedEdges;
            SuperSource = superSource;
            SuperSink = superSink;
            TimeHorizon = TimeHorizon;
        }
    }
}
