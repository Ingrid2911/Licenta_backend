using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal class ExpandedEdge
    {
        //properties
        public string From { get; private set; } //conventie: nod_original + "_" + timp
        public string To { get; private set; }
        public int Capacity { get; private set; }

        //nu mai avem timp, el este deja in nume ex: a_3

        //methods
        public ExpandedEdge(string from, string to, int capacity)
        {
            From = from;
            To = to;
            Capacity = capacity;
        }

    }
}
