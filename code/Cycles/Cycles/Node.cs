using Cycles.Algorithms;
using Neo4j.Driver;

namespace Cycles
{
    public class Node : IEquatable<Node?>
    {
        public HashSet<Relationship> Relations { get; }
        public BFSNodeInfo BFSInfo { get; }
        public NodeLabel Label { get; }
        public Dictionary<(RelationshipLabel Label, RelationshipDirection Direction), long> RelationsCount { get; }

        public long Id { get; }
        public IReadOnlyDictionary<string, object>? Properties { get; }

        public Node(INode inode)
        {
            Id = inode.Id;
            Properties = inode.Properties;
            Relations = new();
            BFSInfo = new(this);
            Label = Enum.Parse<NodeLabel>(inode.Labels[0]);
            RelationsCount = new();
        }

        public Node(long id, List<string> labels)
        {
            Id = id;
            Properties = null;
            Relations = new();
            BFSInfo = new(this);
            Label = Enum.Parse<NodeLabel>(labels[0]);
            RelationsCount = new();
        }

        public void AddRelationsCount(RelationshipLabel label, RelationshipDirection direction, long count)
        {
            RelationsCount.Add((label, direction), count);
        }

        public override bool Equals(object? obj) => Equals(obj as Node);

        public bool Equals(Node? other) => Id.Equals(other?.Id);

        public override int GetHashCode() => Id.GetHashCode();

        public string GetDisplay()
        {
            if (Properties is null)
                throw new NullReferenceException("Properties must be set before calling GetDisplay");

            string proceedingDisplay()
            {
                var key = Properties["dblpKey"].As<string>();
                if (key.StartsWith("conf/"))
                {
                    key = key.Substring("conf/".Length);
                }
                return key;
            }

            return Label switch
            {
                NodeLabel.Article => $"{Properties["Year"].As<int>() % 100} {Properties["Title"].As<string>()}",
                NodeLabel.Conference => Properties["Code"].As<string>(),
                NodeLabel.DOI => Properties["d"].As<string>(),
                NodeLabel.Issue => Properties["wKey"].As<string>(),
                NodeLabel.Journal => Properties["Code"].As<string>(),
                NodeLabel.Orcid => Properties["o"].As<string>(),
                NodeLabel.Person => Properties["Name"].As<string>(),
                NodeLabel.Proceeding => proceedingDisplay(),
                NodeLabel.Volume => Properties["wKey"].As<string>(),
                _ => string.Empty
            };
        }
    }
}
