using Neo4j.Driver;

namespace Cycles.Old
{
    public class Node : PathEntity
    {
        public string Display { get; }

        public Node(long id, char label, string display) : base(id, label)
        {
            Display = display;
        }

        public static Node Create(INode node)
        {
            return new Node(node.Id, LabelMapper.GetLabel(node.Labels[0]), DisplayMapper.GetDisplay(node));
        }
    }
}
