using System.Text;

namespace Cycles.Old
{
    public class MergedCycle
    {
        public List<EqualitySet<Node>> NodeSets { get; private set; }
        public List<HashSet<Relationship>> Relationships { get; private set; }
        public string Label { get; }
        public int OrderLevel { get; }

        public MergedCycle(Cycle cycle)
        {
            NodeSets = cycle.Nodes.Select(n => new EqualitySet<Node>(new HashSet<Node>()) { n }).ToList();
            Relationships = cycle.Relationships.Select(r => new HashSet<Relationship>() { r }).ToList();
            Label = cycle.Label;
            OrderLevel = cycle.OrderLevel;
            //if (OrderLevel == 2)
            //    Mirror();
        }

        private void Mirror()
        {
            for (int i = 0; i < NodeSets.Count / 2; i++)
            {
                var set = NodeSets[i];
                foreach (var mirrorItem in NodeSets[NodeSets.Count - 1 - i])
                {
                    if (!set.Any(n => n.Id == mirrorItem.Id))
                    {
                        set.Add(mirrorItem);
                    }
                }
                NodeSets[NodeSets.Count - 1 - i] = set;
            }
        }

        public bool TryMerge(MergedCycle other, char currentMergeLabel)
        {
            //if (other.OrderLevel != OrderLevel || other.Label != Label || !Label.Contains(currentMergeLabel)) 
            //    return false;

            if (Relationships.Count != other.Relationships.Count || Relationships.Zip(other.Relationships).Any((t) => t.First.First().LeftToRight != t.Second.First().LeftToRight))
                return false;

            int? diffPos = null;

            var limit = NodeSets.Count;

            for (var pos = 0; pos < limit; pos++)
            {
                if (NodeSets[pos] != other.NodeSets[pos])
                {
                    if (diffPos.HasValue || Label[pos * 2] != currentMergeLabel)
                        return false;
                    diffPos = pos;
                }
            }

            if (!diffPos.HasValue)
                return false;

            //Merge
            NodeSets[diffPos.Value].UnionWith(other.NodeSets[diffPos.Value]); //Add nodes to merge
            other.NodeSets = NodeSets; //Merge reference

            Relationships[diffPos.Value - 1].UnionWith(other.Relationships[diffPos.Value - 1]); //Merge weight
            if (Relationships.Count > diffPos.Value)
                Relationships[diffPos.Value].UnionWith(other.Relationships[diffPos.Value]); //Merge weight

            other.Relationships = Relationships; //Merge reference

            return true;
        }

        public string GetCypher()
        {
            var nodeName = 'a';
            var builder = new StringBuilder();
            builder.Append("MATCH P=");

            var nodeEnumerator = NodeSets.GetEnumerator();
            foreach (var relation in Relationships)
            {
                nodeEnumerator.MoveNext();
                builder.Append($"({nodeName.IncrAfter()})");
                builder.Append(GetCypherMatch(relation.First()));
            }
            nodeEnumerator.MoveNext();
            builder.Append($"({nodeName.IncrAfter()})");

            nodeName = 'a';
            builder.Append(" WHERE ");
            builder.Append(string.Join(" AND ", NodeSets.Select(n => GetCypherWhere(n, nodeName.IncrAfter()))));
            builder.Append(" RETURN P");
            return builder.ToString();
        }

        private string GetCypherMatch(Relationship r)
        {
            return $"-[:{LabelMapper.GetType(r.Label)}]-";
        }

        private string GetCypherWhere(IEnumerable<Node> nodes, char name)
        {
            return $"ID({name}) IN [{string.Join(", ", nodes.Select(n => n.Id))}]";
        }

        //public override string ToString()
        //{
        //    var node_ts = (List<Node> nodes) => $"({nodes.First().Label}:{(nodes.Count == 1 ? $"{{{nodes[0].Display}}}" : nodes.Count)})";
        //    var r_ts = (Relationship r) => $"{(!r.LeftToRight ? "<-" : "-")}[{r.Label}:{r.Weight:0.000}]{(r.LeftToRight ? "->" : "-")}";

        //    var node_start_pos = new List<int>();

        //    var builder = new StringBuilder();
        //    var nodeEnumerator = NodeSets.GetEnumerator();
        //    foreach(var relation in Relationships)
        //    {
        //        nodeEnumerator.MoveNext();
        //        node_start_pos.Add(builder.Length);
        //        builder.Append(node_ts(nodeEnumerator.Current));
        //        builder.Append(r_ts(relation));
        //    }
        //    nodeEnumerator.MoveNext();
        //    node_start_pos.Add(builder.Length);
        //    builder.Append(node_ts(nodeEnumerator.Current));

        //    for(int i = 0; i < NodeSets.Count; i++)
        //    {
        //        var nodeSet = NodeSets[i];
        //        if (nodeSet.Count > 1)
        //        {
        //            foreach(var node in nodeSet)
        //            {
        //                builder.Append('\n');
        //                builder.Insert(builder.Length, " ", node_start_pos[i]);
        //                builder.Append('{');
        //                builder.Append(node.Display);
        //                builder.Append('}');
        //            }
        //        }
        //    }

        //    return builder.ToString();
        //}
    }
}
