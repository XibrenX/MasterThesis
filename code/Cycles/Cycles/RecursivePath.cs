namespace Cycles
{
    public class RecursivePathSegment
    {
        public PathPattern Pattern { get; }

        public Node StartNode => Relationship.LeftNode;
        public Node NextNode => Relationship.RightNode;

        public PathRelationship Relationship { get; }
        public RecursivePath? To { get; }

        public RecursivePathSegment(PathRelationship relationship)
        {
            Relationship = relationship;
            To = null;
            Pattern = new PathPattern(new[] { Relationship.LeftNode.Label, Relationship.RightNode.Label }, new[] { Relationship.Relationship.Label }, new[] { Relationship.LeftNodeDirection });
        }

        public RecursivePathSegment(PathRelationship relationship, RecursivePath to)
        {
            Relationship = relationship;
            To = to;

            if (relationship.RightNode != to.StartNode)
                throw new ArgumentException($"{nameof(relationship)}.{nameof(relationship.RightNode)} does not point to {nameof(to)}.{nameof(to.StartNode)}");

            Pattern = relationship + to.Pattern;
        }

        public long GetPathCount()
        {
            return To?.GetPathCount() ?? 1L;
        }
    }

    public class RecursivePath
    {
        public PathPattern Pattern { get; }

        public List<RecursivePathSegment> Segments { get; }

        public Node StartNode { get; }

        public RecursivePath(RecursivePathSegment segment)
        {
            StartNode = segment.StartNode;
            Pattern = segment.Pattern;
            Segments = new List<RecursivePathSegment>(1) { segment };
        }

        public RecursivePath(PathPattern pattern, Node node)
        {
            StartNode = node;
            Pattern = pattern;
            Segments = new List<RecursivePathSegment>();
        }

        public void Add(RecursivePathSegment segment)
        {
            if (segment.StartNode == StartNode && segment.Pattern == Pattern)
            {
                Segments.Add(segment);
            }
            //else error!!!
        }

        public long GetPathCount()
        {
            return Segments.Sum(s => s.GetPathCount());
        }
    }
}
