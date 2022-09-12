namespace DBLPtoCSV
{
    public class WWW : IEquatable<WWW?>
    {
        public string Key { get; }

        public HashSet<Person> Names { get; private set; }
        public List<string> Urls { get; private set; }
        public List<string> Notes { get; private set; }

        public WWW(string key, HashSet<Person> names, string? url, string? note)
        {
            Key = key;
            Names = names;
            Urls = new List<string>();
            if(url != null)
                Urls.Add(url);
            Notes = new List<string>();
            if(note != null)
                Notes.Add(note);
        }

        public WWW TransposeKey(string newKey)
        {
            return new WWW(newKey, Names, null, null)
            {
                Urls = Urls,
                Notes = Notes,
            };
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as WWW);
        }

        public bool Equals(WWW? other)
        {
            if(other is not null && Key == other.Key)
            {
                Names.UnionWith(other.Names);
                other.Names = Names;
                Urls.AddRange(other.Urls);
                other.Urls = Urls;
                Notes.AddRange(other.Notes);
                other.Notes = Notes;
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key);
        }
    }
}
