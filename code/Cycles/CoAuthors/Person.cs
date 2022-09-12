namespace CoAuthors
{
    public class Person : IEquatable<Person?>
    {
        public Person(string name)
        {
            Name = name;
            Coauthors = new();
        }

        public string Name { get; }

        public List<CoAuthors> Coauthors { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person? other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
