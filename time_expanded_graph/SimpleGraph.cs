using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph
{
    internal class SimpleGraph //pe viitor s-ar putea sa il schimb in public
    {
        //properties
        public List<string> Nodes { get; private set; } //List, NU vector<>
        public List <SimpleEdge> Edges{get; private set; }
        public string SourceNode{get; private set;}
        public string SinkNode{get; private set;}

        //methods
        public SimpleGraph(List<string> nodes, List<SimpleEdge> edges, string sourceNode, string sinkNode)
        {
            this.Nodes = nodes;
            this.Edges = edges; 
            this.SourceNode = sourceNode;
            this.SinkNode = sinkNode;
        }
    }
}
