namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    public static class NodeTypeHelper
    {
        public static bool IsTimeExpandedNode(string node)
        {
            int lastUnderscore = node.LastIndexOf('_');

            if (lastUnderscore < 0 || lastUnderscore == node.Length - 1)
                return false;

            string timePart = node.Substring(lastUnderscore + 1);

            return int.TryParse(timePart, out _);
        }
        public static string GetOriginalNodeId(string expandedNode)
        {
            int lastUnderscore = expandedNode.LastIndexOf('_');

            if (lastUnderscore < 0)
                return expandedNode;

            return expandedNode.Substring(0, lastUnderscore);
        }
        public static int GetTimeFromExpandedNode(string expandedNode)
        {
            int lastUnderscore = expandedNode.LastIndexOf('_');

            if (lastUnderscore < 0 || lastUnderscore == expandedNode.Length - 1)
                return 0;

            string timePart = expandedNode.Substring(lastUnderscore + 1);

            return int.TryParse(timePart, out int time)
                ? time
                : 0;
        }
        public static int GetNodeTypePriority(string node)
        {
            if (node.Equals("s", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("S_", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("START", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (node.StartsWith("CAM", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("ROOM", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (node.StartsWith("SC", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("STAIR", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (node.StartsWith("LIFT", StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (node.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("T_", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                return 4;
            }

            return 2;
        }
        public static int ExtractNumberFromNode(string node)
        {
            string digits = new string(node.Where(char.IsDigit).ToArray());

            return int.TryParse(digits, out int value)
                ? value
                : int.MaxValue;
        }
    }
}