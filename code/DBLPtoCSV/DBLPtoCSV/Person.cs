namespace DBLPtoCSV
{
    public class Person : IEquatable<Person?>
    {
        public string dblpName { get; }
        public string Name { get; }
        public HashSet<string> Orcid { get; set; }

        public Person(string dblpName, string? orcid) {
            this.dblpName = dblpName;
            Name = dblpName;
            var lastNamePart = this.dblpName[(this.dblpName.LastIndexOf(' ')+1)..];
            if (uint.TryParse(lastNamePart, out _))
            {
                Name = this.dblpName[..^(lastNamePart.Length + 1)];
            }
            Orcid = new HashSet<string>();
            if(orcid is not null)
                Orcid.Add(orcid);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person? other)
        {
            if(other is not null && dblpName == other.dblpName)
            {
                Orcid.UnionWith(other.Orcid);
                other.Orcid = Orcid;
                return true;
            } else {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(dblpName);
        }
    }
}
