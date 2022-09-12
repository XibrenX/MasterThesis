using Neo4j.Driver;
using Neo4jClient;

namespace Cycles.Old
{

    public class Cycle : IEquatable<Cycle?>
    {
        public Cycle(List<Node> nodes, List<Relationship> relationships)
        {
            Nodes = nodes;
            Relationships = relationships;

            PathEntities = Extensions.Intertwine<PathEntity>(Nodes, Relationships).ToList();

            if (ShouldSwap())
            {
                Nodes.Reverse();
                Relationships.Reverse();
                PathEntities.Reverse();
                foreach (var relationship in Relationships) { relationship.Swap(); }
            }

            Label = string.Join(null, PathEntities.Select(pe => pe.Label));

            if (OrderLevel == 2)
            {
                int nodeRemoveStart = (Nodes.Count + 1) / 2;
                Nodes.RemoveRange(nodeRemoveStart, Nodes.Count - nodeRemoveStart);
                Relationships.RemoveRange(Relationships.Count / 2, Relationships.Count / 2);
            }
        }

        public List<Node> Nodes { get; }
        public List<Relationship> Relationships { get; }
        public List<PathEntity> PathEntities { get; }

        public string Label { get; }
        public int OrderLevel { get; private set; }

        private bool ShouldSwap()
        {
            OrderLevel = 0;
            for (var i = 1; i <= PathEntities.Count / 2; i++)
            {
                var first = PathEntities[i];
                var reverse = PathEntities[PathEntities.Count - i - 1];

                var comp = first.Label.CompareTo(reverse.Label);
                if (comp == 0)
                    continue;
                return comp > 0;
            }

            OrderLevel = 1;
            for (var i = 0; i < Relationships.Count / 2; i++)
            {
                var first = Relationships[i];
                var reverse = Relationships[Relationships.Count - i - 1];

                var comp = first.LeftToRight.CompareTo(!reverse.LeftToRight);
                if (comp == 0)
                    continue;
                return comp < 0;
            }

            if (Relationships.Count % 2 == 1)
            {
                return !Relationships[Relationships.Count / 2].LeftToRight;
            }

            OrderLevel = 2;
            //for (var i = 1; i <= PathEntities.Count / 2; i++)
            //{
            //    var first = PathEntities[i];
            //    var reverse = PathEntities[PathEntities.Count - i - 1];

            //    var comp = first.Id.CompareTo(reverse.Id); ;  
            //    if (comp == 0)
            //        continue;
            //    return comp > 0;
            //}
            return false;
        }

        public static Cycle Create(IPath cycle, NodesManager nodesManager)
        {
            var nodes = new List<Node>();
            var relationships = new List<Relationship>();

            nodes.Add(nodesManager.Add(cycle.Start));
            foreach (var relationship in cycle.Relationships)
            {
                var leftNodeId = nodes.Last().Id;
                relationships.Add(Relationship.Create(relationship, leftNodeId));
                var rightNodeId = relationship.StartNodeId == leftNodeId ? relationship.EndNodeId : relationship.StartNodeId;
                nodes.Add(nodesManager.Add(cycle.Nodes.First(n => n.Id == rightNodeId)));
            }

            return new Cycle(nodes, relationships);
        }

        public async Task AddWeights(NodeRelationsCache cache)
        {
            var nodeEnumerator = Nodes.GetEnumerator();
            nodeEnumerator.MoveNext();
            var leftNode = nodeEnumerator.Current;

            foreach (var relation in Relationships)
            {
                var relationType = LabelMapper.GetType(relation.Label);
                var leftTotalRelationsOfType = await cache.GetRelationshipCount(leftNode.Id, relationType, relation.LeftToRight ? RelationshipDirection.Outgoing : RelationshipDirection.Incoming);

                nodeEnumerator.MoveNext();
                leftNode = nodeEnumerator.Current;
                var rightTotalRelationsOfType = await cache.GetRelationshipCount(leftNode.Id, relationType, !relation.LeftToRight ? RelationshipDirection.Outgoing : RelationshipDirection.Incoming);

                relation.LeftTotalRelationships = leftTotalRelationsOfType;
                relation.RightTotalRelationships = rightTotalRelationsOfType;
                //relation.Weight = (1.0 / leftTotalRelationsOfType) * (1.0 / rightTotalRelationsOfType);
            }
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Cycle);
        }

        public bool Equals(Cycle? other)
        {
            return other != null &&
                   SequenceEqualityComparer<PathEntity>.Default.Equals(PathEntities, other.PathEntities);
        }

        public override int GetHashCode()
        {
            return SequenceEqualityComparer<PathEntity>.Default.GetHashCode(PathEntities);
        }
    }
}

