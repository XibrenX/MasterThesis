namespace DBLPtoCSV
{
    public class Serie : IEquatable<Serie?>
    {
        public string Name { get; }
        public string? Url { get; }

        public Serie(string name, string? url) {
            Name = name;
            Url = url;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Serie);
        }

        public bool Equals(Serie? other)
        {
            return other is not null &&
                   Name == other.Name &&
                   Url == other.Url;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Url);
        }
    }
}
