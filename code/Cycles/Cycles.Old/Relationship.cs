using Neo4j.Driver;
using static Neo4jClient.ApiModels.Cypher.PathsResultBolt;

namespace Cycles.Old
{
    public class Relationship : PathEntity
    {
        public bool LeftToRight { get; set; }

        public long LeftTotalRelationships { get; set; } = 0;
        public long RightTotalRelationships { get; set; } = 0;

        public Relationship(long id, char label, bool leftToRight) : base(id, label)
        {
            LeftToRight = leftToRight;
        }

        public void Swap()
        {
            LeftToRight = !LeftToRight;
            (LeftTotalRelationships, RightTotalRelationships) = (RightTotalRelationships, LeftTotalRelationships);
        }

        public static Relationship Create(IRelationship relationship, long leftNodeId)
        {
            return new Relationship(relationship.Id, LabelMapper.GetLabel(relationship.Type), relationship.StartNodeId == leftNodeId);
        }
    }
}
