using Neo4j.Driver;
using System.Diagnostics.CodeAnalysis;

namespace Cycles
{
    public class Relationship : IEquatable<Relationship?>
    {
        public RelationshipLabel Label { get; }
        public Node? StartNode { get; private set; }
        public Node? EndNode { get; private set; }

        public PathRelationship StartToEnd => new(this, StartNode);
        public PathRelationship EndToStart => new(this, EndNode);

        [MemberNotNullWhen(true, nameof(StartNode), nameof(EndNode), nameof(StartToEnd), nameof(EndToStart))]
        public bool IsAttached => StartNode is not null && EndNode is not null;
        public long Id { get; }
        public long StartNodeId { get; }
        public long EndNodeId { get; }

        public Relationship(IRelationship irelationship)
        {
            Id = irelationship.Id;
            StartNodeId = irelationship.StartNodeId;
            EndNodeId = irelationship.EndNodeId;
            Label = LabelsExtensions.RelationshipLabelFromDB(irelationship.Type);
        }

        public Relationship(long id, long startNodeId, long endNodeId, string type)
        {
            Id = id;
            StartNodeId = startNodeId;
            EndNodeId = endNodeId;
            Label = LabelsExtensions.RelationshipLabelFromDB(type);
        }

        [MemberNotNullWhen(true, nameof(StartNode), nameof(EndNode), nameof(StartToEnd), nameof(EndToStart))]
        public bool TryAttach(Dictionary<long, Node> nodeDir)
        {
            if (nodeDir.TryGetValue(StartNodeId, out Node? startNode) && nodeDir.TryGetValue(EndNodeId, out Node? endNode))
            {
                StartNode = startNode;
                EndNode = endNode;

                startNode.Relations.Add(this);
                endNode.Relations.Add(this);

                var distance = Math.Min(startNode.BFSInfo.Distance, endNode.BFSInfo.Distance) + 1;
                if (distance == 0) // overflow
                {
                    distance = uint.MaxValue;
                }
                startNode.BFSInfo.SetDistance(distance);
                endNode.BFSInfo.SetDistance(distance);
                return true;
            }
            else
            {
                return false;
            }
        }

        [MemberNotNull(nameof(StartNode), nameof(EndNode), nameof(StartToEnd), nameof(EndToStart))]
        public void Attach(Dictionary<long, Node> nodeDir)
        {
            if (!TryAttach(nodeDir))
                throw new InvalidOperationException($"Could not find startNode or endNode");
        }

        public void Detach()
        {
            if (StartNode is not null)
            {
                StartNode.Relations.Remove(this);
                StartNode = null;
            }
            if (EndNode is not null)
            {
                EndNode.Relations.Remove(this);
                EndNode = null;
            }
        }

        [MemberNotNull(nameof(StartNode), nameof(EndNode), nameof(StartToEnd), nameof(EndToStart))]
        public RelationshipDirection GetDirection(Node node)
        {
            if (IsAttached)
            {
                if (node.Equals(StartNode))
                    return RelationshipDirection.Out;
                else if (node.Equals(EndNode))
                    return RelationshipDirection.In;
                else
                    throw new ArgumentException($"{nameof(node)} is not a start or end node in this relation", nameof(node));
            }
            else
                throw new InvalidOperationException("Relationship not attached");
        }

        [MemberNotNull(nameof(StartNode), nameof(EndNode), nameof(StartToEnd), nameof(EndToStart))]
        public Node GetOtherNode(Node node)
        {
            if (IsAttached)
            {
                if (node.Equals(StartNode))
                    return EndNode;
                else if (node.Equals(EndNode))
                    return StartNode;
                else
                    throw new ArgumentException($"{nameof(node)} is not a start or end node in this relation", nameof(node));
            }
            else
                throw new InvalidOperationException("Relationship not attached");
        }

        [MemberNotNull(nameof(StartNode), nameof(EndNode), nameof(StartToEnd), nameof(EndToStart))]
        public PathRelationship GetPathRelationship(Node leftNode)
        {
            if (IsAttached)
            {
                if (leftNode.Equals(StartNode))
                    return StartToEnd;
                else if (leftNode.Equals(EndNode))
                    return EndToStart;
                else
                    throw new ArgumentException($"{nameof(leftNode)} is not a start or end node in this relation", nameof(leftNode));
            }
            else
                throw new InvalidOperationException("Relationship not attached");
        }

        public override bool Equals(object? obj) => Equals(obj as Relationship);

        public bool Equals(Relationship? other) => Id.Equals(other?.Id);

        public override int GetHashCode() => Id.GetHashCode();
    }
}
