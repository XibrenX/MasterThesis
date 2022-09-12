using Cycles;
using Neo4j.Driver;

namespace Cycles.Algorithms
{
    public abstract class InfoOptions
    {
        protected string? nodeName;
        protected string? relationName;

        public virtual void SetNames(string nodeName, string relationName)
        {
            this.nodeName = nodeName;
            this.relationName = relationName;
        }

        public abstract void Construct(IRecord record, Dictionary<long, Node> nodesDir);

        public abstract string GetCyper();
    }

    public class FullInfoOptions : InfoOptions
    {
        public override void Construct(IRecord record, Dictionary<long, Node> nodesDir)
        {
            if (record[nodeName] is INode n && !nodesDir.ContainsKey(n.Id))
            {
                nodesDir.Add(n.Id, new Node(n));
            }

            if (record[relationName] is IRelationship e)
            {
                var relationship = new Relationship(e);
                //if (filter.AllowedRelationshipLabels.Contains(relationship.Label))
                relationship.TryAttach(nodesDir);
            }
        }

        public override string GetCyper()
        {
            return $"{relationName}, {nodeName}";
        }
    }

    public class LimitedInfoOptions : InfoOptions
    {
        private const string NODE_ID = "ni";
        private const string NODE_LABELS = "nl";

        public override void Construct(IRecord record, Dictionary<long, Node> nodesDir)
        {
            long nodeId = record[NODE_ID].As<long>();

            if (!nodesDir.ContainsKey(nodeId))
            {
                var nodeLabels = record[NODE_LABELS].As<List<string>>();
                nodesDir.Add(nodeId, new Node(nodeId, nodeLabels));
            }

            if (record[relationName] is IRelationship e)
            {
                var relationship = new Relationship(e);
                //if (filter.AllowedRelationshipLabels.Contains(relationship.Label))
                relationship.TryAttach(nodesDir);
            }
        }

        public override string GetCyper()
        {
            return $"{relationName}, ID({nodeName}) AS {NODE_ID}, labels({nodeName}) AS {NODE_LABELS}";
        }
    }
}