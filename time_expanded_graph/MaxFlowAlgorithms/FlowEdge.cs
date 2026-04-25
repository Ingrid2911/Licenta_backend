using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph.MaxFlowAlgorithms
{
    internal class FlowEdge
    {
        public int To;
        public int Capacity;
        public int Flow;
        public FlowEdge Reverse;

        public FlowEdge(int to, int capacity)
        {
            To = to;
            Capacity = capacity;
            Flow = 0;
        }

        public int ResidualCapacity()
        {
            return Capacity - Flow;
        }

    }
}
