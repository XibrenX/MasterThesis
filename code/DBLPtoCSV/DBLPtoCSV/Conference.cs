namespace DBLPtoCSV
{
    public class Conference : IEquatable<Conference?>
    {
        private Conference(string dblpKey, string code)
        {
            DblpKey = dblpKey;
            Code = code;
        }

        public string DblpKey { get; set; }
        public string Code { get; set; }

        

        public override bool Equals(object? obj)
        {
            return Equals(obj as Conference);
        }

        public bool Equals(Conference? other)
        {
            return other != null &&
                   DblpKey == other.DblpKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DblpKey);
        }

        public static Conference? GetConference(string dblpKey)
        {
            var parts = dblpKey.Split('/');
            if(parts.Length >= 2)
            {
                var confDblpKey = parts[0] + '/' + parts[1];
                var code = parts[1];
                return new Conference(confDblpKey, code);
            }
            return null;
        }
    }
}
