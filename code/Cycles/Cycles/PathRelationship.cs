namespace Cycles
{
    public readonly struct PathRelationship : IEquatable<PathRelationship?>
    {
        public Relationship Relationship { get; }
        public RelationshipDirection LeftNodeDirection { get; }

        public Node LeftNode { get => LeftNodeDirection == RelationshipDirection.Out ? Relationship.StartNode : Relationship.EndNode; }
        public Node RightNode { get => LeftNodeDirection == RelationshipDirection.Out ? Relationship.EndNode : Relationship.StartNode; }

        public long Id => Relationship.Id;

        public PathRelationship(Relationship relationship, Node leftNode)
        {
            Relationship = relationship;
            LeftNodeDirection = relationship.GetDirection(leftNode);
        }

        public PathRelationship(Relationship relationship, RelationshipDirection leftNodeDirection)
        {
            Relationship = relationship;
            LeftNodeDirection = leftNodeDirection;
        }

        public PathRelationship Inverse() => new(Relationship, LeftNodeDirection.Inverse());

        public override bool Equals(object? obj) => Equals(obj as PathRelationship?);

        public bool Equals(PathRelationship? other) =>
            other != null &&
            Relationship.Equals(other.Value.Relationship) &&
            LeftNodeDirection == other.Value.LeftNodeDirection;

        public override int GetHashCode() => HashCode.Combine(Relationship, LeftNodeDirection);

        public static bool operator ==(PathRelationship left, PathRelationship right) => left.Equals(right);

        public static bool operator !=(PathRelationship left, PathRelationship right) => !(left == right);
    }
}
