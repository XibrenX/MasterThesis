using System.Text;

namespace Cycles
{
    public class RecursiveCycle
    {
        public RecursivePath PathA { get; protected set; }
        public RecursivePath PathB { get; protected set; }

        public PathPattern Pattern { get; protected set; }

        public RecursiveCycle(RecursivePath pathA, RecursivePath pathB)
        {
            if (pathA.StartNode != pathB.StartNode)
                throw new ArgumentException($"{nameof(pathA.StartNode)} must be the same for both {nameof(pathA)} as {nameof(pathB)}");

            PathA = pathA;
            PathB = pathB;

            Pattern = PathA.Pattern.Inverse() + PathB.Pattern;
        }

        public void Mirror()
        {
            (PathA, PathB) = (PathB, PathA);
            Pattern = Pattern.Inverse();
        }

        public virtual string GetCypher()
        {
            var nodeIds = GetNodeIds();

            var nodeName = 'a';
            var builder = new StringBuilder();
            builder.Append("MATCH P=");

            builder.Append($"({nodeName.IncrAfter()})");
            for (var i = 0; i < nodeIds.Count - 1; i++)
            {
                builder.Append("--");
                builder.Append($"({nodeName.IncrAfter()})");
            }

            nodeName = 'a';
            builder.Append(" WHERE ");
            builder.Append(string.Join(" AND ", nodeIds.Select(ids => $"ID({nodeName.IncrAfter()}) IN [{string.Join(", ", ids)}]")));
            builder.Append(" RETURN P");
            return builder.ToString();
        }

        protected virtual List<HashSet<long>> GetNodeIds()
        {
            var nodeIdsA = new List<HashSet<long>>();
            AddIds(nodeIdsA, PathA, 0);
            var nodeIdsB = new List<HashSet<long>>();
            AddIds(nodeIdsB, PathB, 0);

            nodeIdsA.Reverse();
            nodeIdsA.RemoveAt(nodeIdsA.Count - 1);
            nodeIdsA.AddRange(nodeIdsB);
            return nodeIdsA;
        }

        protected void AddIds(List<HashSet<long>> ids, RecursivePath rp, int level)
        {
            if (ids.Count == level)
                ids.Add(new HashSet<long>());
            ids[level].Add(rp.StartNode.Id);

            if (rp.Segments.Count > 0)
            {
                if (ids.Count == level + 1)
                    ids.Add(new HashSet<long>());

                foreach (var segement in rp.Segments)
                {
                    if (segement.To is not null)
                    {
                        AddIds(ids, segement.To, level + 1);
                    }
                    else
                    {
                        ids[level + 1].Add(segement.NextNode.Id);
                    }
                }
            }
        }

        public virtual long GetPathCounts()
        {
            long sum = 0;

            var aDict = PathA.Segments.Select(s => (s.Relationship.Id, s.GetPathCount()))
                .GroupBy(s => s.Id)
                .ToDictionary(sg => sg.Key, sg => sg.Sum(s => s.Item2));
            var bDict = PathB.Segments.Select(s => (s.Relationship.Id, s.GetPathCount()))
                .GroupBy(s => s.Id)
                .ToDictionary(sg => sg.Key, sg => sg.Sum(s => s.Item2));

            foreach (var (idA, pathCountA) in aDict)
            {
                foreach (var (idB, pathCountB) in bDict)
                {
                    if (idA != idB)
                    {
                        sum += pathCountA * pathCountB;
                    }
                }
            }

            return sum;
        }
    }

    public class MirroredRecursiveCycle : RecursiveCycle
    {
        public RecursivePath Path => PathA;

        public MirroredRecursiveCycle(RecursivePath path) : base(path, path)
        {
            if (path.Segments.Count < 2)
                throw new ArgumentException($"{nameof(path)} does not contain a cycle");
        }

        protected override List<HashSet<long>> GetNodeIds()
        {
            var nodeIds = new List<HashSet<long>>();
            AddIds(nodeIds, Path, 0);
            nodeIds.Reverse();
            return nodeIds;
        }

        public override long GetPathCounts()
        {
            long sum = 0;

            var aDict = PathA.Segments.Select(s => (s.Relationship.Id, s.GetPathCount()))
                .GroupBy(s => s.Id)
                .ToDictionary(sg => sg.Key, sg => sg.Sum(s => s.Item2));

            foreach (var (idA, pathCountA) in aDict)
            {
                foreach (var (idB, pathCountB) in aDict)
                {
                    if (idA != idB)
                    {
                        sum += pathCountA * pathCountB;
                    }
                }
            }

            return sum;
        }
    }
}
