using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph
{
    internal class SimpleEdge //pe viitor s-ar putea sa il schimb in public
    {
        //properties
        public string From { get; private set; }
        public string To { get; private set; }
        public int TravelTime { get; private set; }
        public int Capacity { get; private set; }

        //methods
        public SimpleEdge(string from, string to, int travelTime, int capacity)
        {
            this.From = from;
            this.To = to;
            this.TravelTime = travelTime;
            this.Capacity = capacity;
        }

    }
}
