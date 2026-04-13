using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal class ExpandedGraph
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
