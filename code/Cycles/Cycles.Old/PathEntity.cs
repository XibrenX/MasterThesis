namespace Cycles.Old
{
    public abstract class PathEntity : IEquatable<PathEntity?>
    {
        protected PathEntity(long id, char label)
        {
            Id = id;
            Label = label;
        }

        public long Id { get; }
        public char Label { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as PathEntity);
        }

        public bool Equals(PathEntity? other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
