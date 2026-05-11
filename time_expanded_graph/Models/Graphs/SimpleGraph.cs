using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace time_expanded_graph.Models.Graphs
{
    public class SimpleGraph 
    {
        //properties
        public List<string> Nodes { get; private set; } 
        public List <SimpleEdge> Edges{get; private set; }
        public string SourceNode{get; private set;}
        public string SinkNode{get; private set;}

        //methods
        public SimpleGraph(List<string> nodes, List<SimpleEdge> edges, string sourceNode, string sinkNode)
        {
            Nodes = nodes;
            Edges = edges; 
            SourceNode = sourceNode;
            SinkNode = sinkNode;
        }
    }
}
