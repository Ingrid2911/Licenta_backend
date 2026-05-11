using System.Diagnostics;
using time_expanded_graph.Models.Algorithms;
using time_expanded_graph.Models.Graphs;
using time_expanded_graph.Models.Builders;

namespace time_expanded_graph.Models.Utilities
{
    internal class BinarySearch
    {
        public static int FindMinimumTime(SimpleGraph graph, int peopleNeeded, int maxTime)
        {
            int low = 1;
            int high = maxTime;
            int answer = -1;

            while (low <= high)
            {
                int mid = (low + high) / 2;

                int flow = ComputeFlow(graph, mid);

                Debug.WriteLine($"T={mid} → flow={flow}");

                if (flow >= peopleNeeded)
                {
                    answer = mid;
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }

            return answer;
        }

        private static int ComputeFlow(SimpleGraph graph, int time)
        {
            var builder = new TimeExpandedGraphBuilder();
            var expanded = builder.BuildTimeExpandedGraph(graph, time);

            var dinic = new DinicSolver(expanded);
            return dinic.ComputeMaxFlow("S*", "T*");
        }
    }
}