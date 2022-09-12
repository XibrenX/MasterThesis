using Neo4j.Driver;

namespace Cycles.Old
{
    public class NodesManager
    {
        private Dictionary<long, Node> Nodes { get; }

        public NodesManager()
        {
            Nodes = new Dictionary<long, Node>();
        }

        public Node Add(INode iNode)
        {
            if (!Nodes.TryGetValue(iNode.Id, out var node))
            {
                node = Node.Create(iNode);
                Nodes.Add(iNode.Id, node);
            }
            return node;
        }
    }
}
