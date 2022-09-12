using System.Text;

namespace Cycles
{
    public readonly struct PathPattern : IEquatable<PathPattern>, IComparable<PathPattern>
    {
        private static readonly int NLBitSize = GetBits(GetMax<NodeLabel>());
        private static readonly ulong NLBitMask = GetMask(NLBitSize);
        private static readonly int RLBitSize = GetBits(GetMax<RelationshipLabel>());
        private static readonly ulong RLBitMask = GetMask(RLBitSize);
        private static readonly int DBitSize = GetBits(GetMax<RelationshipDirection>());
        private static readonly ulong DBitMask = GetMask(DBitSize);
        private const int lengthIndex = 60;

        public readonly ulong pattern;

        public int Length => (int)(pattern >> lengthIndex);

        public PathPattern(IEnumerable<NodeLabel> nodeLabels, IEnumerable<RelationshipLabel> relationshipLabels, IEnumerable<RelationshipDirection> relationshipDirections)
        {
            var nodeLabelsCount = 0;
            var relationshipLabelsCount = 0;
            var relationshipDirectionsCount = 0;

            pattern = 0;
            foreach (var nodeLabel in nodeLabels)
            {
                pattern <<= NLBitSize;
                pattern |= (ulong)nodeLabel;
                nodeLabelsCount += 1;
            }
            foreach (var relationshipLabel in relationshipLabels)
            {
                pattern <<= RLBitSize;
                pattern |= (ulong)relationshipLabel;
                relationshipLabelsCount += 1;
            }
            foreach (var relationshipDirection in relationshipDirections)
            {
                pattern <<= DBitSize;
                pattern |= (ulong)relationshipDirection;
                relationshipDirectionsCount += 1;
            }

            if (relationshipLabelsCount != relationshipDirectionsCount)
                throw new ArgumentException($"Size of {nameof(relationshipLabels)}({relationshipLabelsCount}) and {nameof(relationshipDirections)}({relationshipDirectionsCount}) must be equal");
            if (nodeLabelsCount != relationshipLabelsCount + 1)
                throw new ArgumentException($"Size of {nameof(nodeLabels)}({nodeLabelsCount}) must be the size of {nameof(relationshipLabels)}({relationshipLabelsCount}) + 1");
            if (nodeLabelsCount * NLBitSize + relationshipLabelsCount * (RLBitSize + DBitSize) > lengthIndex)
                throw new ArgumentException($"Size of {nameof(relationshipLabelsCount)}({relationshipLabelsCount}) is too big");

            pattern |= (ulong)relationshipLabelsCount << lengthIndex;
        }

        private PathPattern(ulong pattern)
        {
            this.pattern = pattern;
        }

        public (List<NodeLabel> NodeLabels, List<RelationshipLabel> RelationshipLabels, List<RelationshipDirection> RelationshipDirections) Extract()
        {
            var length = Length;
            var directionStart = length;
            var relationStart = directionStart + RLBitSize * length;
            var nodeStart = relationStart + NLBitSize * (length + 1);

            var nodeLabels = new List<NodeLabel>();
            var relationshipLabels = new List<RelationshipLabel>();
            var relationshipDirections = new List<RelationshipDirection>();

            for (int i = 0; i < length + 1; i++)
            {
                nodeLabels.Add(GetNode(nodeStart, i));
            }

            for (int i = 0; i < length; i++)
            {
                relationshipLabels.Add(GetRelation(relationStart, i));
                relationshipDirections.Add(GetDirection(directionStart, i));
            }

            return (nodeLabels, relationshipLabels, relationshipDirections);
        }

        public override bool Equals(object? obj) => obj is PathPattern pattern && Equals(pattern);

        public bool Equals(PathPattern other) => pattern == other.pattern;

        public override int GetHashCode() => HashCode.Combine(pattern);

        public int CompareTo(PathPattern other) => pattern.CompareTo(other.pattern);

        public static bool operator ==(PathPattern left, PathPattern right) => left.Equals(right);

        public static bool operator !=(PathPattern left, PathPattern right) => !(left == right);

        public static bool operator <(PathPattern left, PathPattern right) => left.CompareTo(right) < 0;

        public static bool operator <=(PathPattern left, PathPattern right) => left.CompareTo(right) <= 0;

        public static bool operator >(PathPattern left, PathPattern right) => left.CompareTo(right) > 0;

        public static bool operator >=(PathPattern left, PathPattern right) => left.CompareTo(right) >= 0;

        public static PathPattern operator +(PathRelationship left, PathPattern right)
        {
            var (NodeLabels, RelationshipLabels, RelationshipDirections) = right.Extract();
            if (NodeLabels[0] != left.RightNode.Label)
                throw new ArgumentException($"This path pattern starts with a {NodeLabels[0]} node but the relation ends with a {left.RightNode.Label} node");
            NodeLabels.Insert(0, left.LeftNode.Label);
            RelationshipLabels.Insert(0, left.Relationship.Label);
            RelationshipDirections.Insert(0, left.LeftNodeDirection);

            return new PathPattern(NodeLabels, RelationshipLabels, RelationshipDirections);
        }

        public static PathPattern operator +(PathPattern left, PathPattern right)
        {
            var (NodeLabels, RelationshipLabels, RelationshipDirections) = left.Extract();
            var extractedRight = right.Extract();

            if (NodeLabels[^1] != extractedRight.NodeLabels[0])
                throw new ArgumentException($"The left ends with a {NodeLabels[^1]} node, while right starts with a {extractedRight.NodeLabels[0]}, they must be the same");

            NodeLabels.RemoveAt(NodeLabels.Count - 1);
            NodeLabels.AddRange(extractedRight.NodeLabels);
            RelationshipLabels.AddRange(extractedRight.RelationshipLabels);
            RelationshipDirections.AddRange(extractedRight.RelationshipDirections);

            return new PathPattern(NodeLabels, RelationshipLabels, RelationshipDirections);
        }

        public PathPattern Inverse()
        {
            var length = Length;
            ulong newPattern = 0;
            var directionStart = length;
            var relationStart = directionStart + RLBitSize * length;
            var nodeStart = relationStart + NLBitSize * (length + 1);

            for (int i = length; i >= 0; i--)
            {
                newPattern <<= NLBitSize;
                newPattern |= (ulong)GetNode(nodeStart, i);
            }

            for (int i = length - 1; i >= 0; i--)
            {
                newPattern <<= RLBitSize;
                newPattern |= (ulong)GetRelation(relationStart, i);
            }

            for (int i = length - 1; i >= 0; i--)
            {
                newPattern <<= DBitSize;
                newPattern |= (ulong)GetDirection(directionStart, i).Inverse();
            }
            newPattern |= (ulong)length << lengthIndex;
            return new PathPattern(newPattern);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var length = Length;
            var directionStart = length;
            var relationStart = directionStart + RLBitSize * length;
            var nodeStart = relationStart + NLBitSize * (length + 1);

            sb.Append(GetNode(nodeStart, 0));
            for (var i = 0; i < length; i++)
            {
                var dir = GetDirection(directionStart, i);
                var rType = GetRelation(relationStart, i);
                if (dir == RelationshipDirection.Out)
                {
                    sb.Append($"--{rType}->");
                }
                else
                {
                    sb.Append($"<-{rType}--");
                }
                sb.Append(GetNode(nodeStart, i + 1));
            }

            return sb.ToString();
        }

        public string ToShortString(bool unicode = false)
        {
            var rightArrow = unicode ? '\u0355' : '>';
            var leftArrow = unicode ? '\u0354' : '<';

            var length = Length;
            var sb = new StringBuilder(length * 3 + length + 1);

            var directionStart = length;
            var relationStart = directionStart + RLBitSize * length;
            var nodeStart = relationStart + NLBitSize * (length + 1);

            var lastNodeLabel = GetNode(nodeStart, 0).ToShortString();

            sb.Append(lastNodeLabel);
            for (var i = 0; i < length; i++)
            {
                var dir = GetDirection(directionStart, i);
                var rType = GetRelation(relationStart, i);
                var arrow = dir == RelationshipDirection.Out ? rightArrow : leftArrow;
                var nextNodeLabel = GetNode(nodeStart, i + 1).ToShortString();
                sb.Append(rType.ToShortString());
                if (lastNodeLabel == nextNodeLabel)
                    sb.Append(arrow);
                sb.Append(nextNodeLabel);
                lastNodeLabel = nextNodeLabel;
            }

            return sb.ToString();
        }

        public string ToLabelString()
        {
            var length = Length;
            var sb = new StringBuilder(length + length + 1);

            var directionStart = length;
            var relationStart = directionStart + RLBitSize * length;
            var nodeStart = relationStart + NLBitSize * (length + 1);

            sb.Append(GetNode(nodeStart, 0).ToShortString());
            for (var i = 0; i < length; i++)
            {
                sb.Append(GetRelation(relationStart, i).ToShortString());
                sb.Append(GetNode(nodeStart, i + 1).ToShortString());
            }

            return sb.ToString();
        }

        public bool LabelEquals(PathPattern? other)
        {
            if (other is null) return false;
            var length = (int)(pattern >> lengthIndex);
            return ((pattern ^ other.Value.pattern) & ~GetMask(length)) == 0UL;
        }

        private readonly NodeLabel GetNode(int nodeStart, int i) => (NodeLabel)(pattern >> nodeStart - NLBitSize * (i + 1) & NLBitMask);

        private readonly RelationshipDirection GetDirection(int directionStart, int i) => (RelationshipDirection)(pattern >> directionStart - DBitSize * (i + 1) & DBitMask);

        private readonly RelationshipLabel GetRelation(int relationStart, int i) => (RelationshipLabel)(pattern >> relationStart - RLBitSize * (i + 1) & RLBitMask);

        private static ulong GetMask(int count) => (1UL << count) - 1;
        private static int GetMax<T>() where T : struct, Enum => Enum.GetValues<T>().Select(v => Convert.ToInt32(v)).Max();
        private static int GetBits(int value)
        {
            int c = 0;
            while (value != 0)
            {
                value = value / 2;
                c += 1;
            }
            return c;
        }
    }
}
