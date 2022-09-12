//using System.Text;

//namespace Cycles2
//{
//    [Obsolete("Old class")]
//    public class Path
//    {
//        public List<Node> Nodes { get; }
//        public List<PathRelationship> Relationships { get; }
//        public uint Length { get => (uint)Relationships.Count; }
//        private PathPattern? pattern = null;
//        public PathPattern Pattern
//        {
//            get
//            {
//                if (pattern is null)
//                {
//                    pattern = new PathPattern(Nodes.Select(n => n.Label), Relationships.Select(r => r.Relationship.Label), Relationships.Select(r => r.LeftNodeDirection));
//                }
//                return pattern.Value;
//            }
//        }

//        private Path(List<Node> nodes, List<PathRelationship> relationships)
//        {
//            Nodes = nodes;
//            Relationships = relationships;
//        }
//        public Path(Node startNode)
//        {
//            Nodes = new() { startNode };
//            Relationships = new();
//        }

//        public void Mirror()
//        {
//            pattern = pattern?.Inverse();
//            Nodes.Reverse();
//            Relationships.Reverse();
//            for(var i = 0; i < Relationships.Count; i++)
//            {
//                Relationships[i] = Relationships[i].Inverse();
//            }    
//        }

//        public Path GetCycle(Node node)
//        {
//            var index = Nodes.IndexOf(node);
//            if (index == 0)
//                return this;

//            return new Path(Nodes.GetRange(index, Nodes.Count - index), Relationships.GetRange(index, Relationships.Count - index));
//        }

//        public Path CloneAndAddSegement(Relationship relationship)
//        {
//            var clone = new Path(Nodes.ToList(), Relationships.ToList());
//            var leftNode = Nodes[^1];
//            var pathRelationship = new PathRelationship(relationship, leftNode);
//            clone.Relationships.Add(pathRelationship);
//            clone.Nodes.Add(pathRelationship.RightNode);
//            return clone;
//        }

//        public static Path Combine(Path a, Path b)
//        {
//            if (a.Nodes[^1] != b.Nodes[^1])
//                throw new ArgumentException($"Path {nameof(b)} does not end with end of Path {nameof(a)}", nameof(b));

//            var nodes = new List<Node>(a.Nodes.Count + b.Nodes.Count - 1);
//            nodes.AddRange(a.Nodes);
//            for(var i = b.Nodes.Count - 2; i >= 0; i--)
//            {
//                nodes.Add(b.Nodes[i]);
//            }

//            var relations = new List<PathRelationship>(a.Relationships.Count + b.Relationships.Count);
//            relations.AddRange(a.Relationships);
//            for(var i = b.Relationships.Count - 1; i >= 0; i--)
//            {
//                relations.Add(b.Relationships[i].Inverse());
//            }

//            return new Path(nodes, relations);
//        }

//        public string GetCypher()
//        {
//            var nodeName = 'a';
//            var builder = new StringBuilder();
//            builder.Append("MATCH P=");

//            builder.Append($"({nodeName.IncrAfter()})");
//            for (var i = 0; i < Relationships.Count; i++)
//            {
//                builder.Append("--");
//                builder.Append($"({nodeName.IncrAfter()})");
//            }

//            nodeName = 'a';
//            builder.Append(" WHERE ");
//            builder.Append(string.Join(" AND ", Nodes.Select(n => $"ID({nodeName.IncrAfter()}) = {n.Id}")));
//            builder.Append(" RETURN P");
//            return builder.ToString();
//        }
//    }
//}
