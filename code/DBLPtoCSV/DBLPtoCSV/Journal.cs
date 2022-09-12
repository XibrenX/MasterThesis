using System.Text.RegularExpressions;

namespace DBLPtoCSV
{
    public static class WKey
    {
        public const char Seperator = '#';
        private static readonly Regex truncateRegex = new(Seperator + "*$", RegexOptions.Compiled); 

        public static string Construct(params string?[] names) => string.Join(Seperator, names);
        public static string Truncate(string wKey) => truncateRegex.Replace(wKey, string.Empty);
    }

    public class Journal : IEquatable<Journal?>
    {
        private static ulong lastId = 0;

        public Journal(string title, string? publisher, string? code)
        {
            Title = title;
            Publisher = publisher;
            Code = code;
            wKey = lastId++;
        }

        public string Title { get; }
        public string? Code { get; private set; }
        public string? Publisher { get; private set ; }
        public ulong wKey { get; private set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Journal);
        }

        public bool Equals(Journal? other)
        {
            if (other is not null && Title == other.Title)
            {
                Publisher ??= other.Publisher;
                other.Publisher ??= Publisher;
                Code ??= other.Code;
                other.Code ??= Code;
                var id = Math.Min(wKey, other.wKey);
                wKey = id;
                other.wKey = id;

                return true;
            } else {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title);
        }
    }

    public class Volume : IEquatable<Volume?>
    {
        public Volume(ulong? journalKey, string volumeName)
        {
            wKey = WKey.Construct(journalKey?.ToString(), volumeName);
            VolumeName = volumeName;
        }

        public string wKey { get; }
        public string VolumeName { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Volume);
        }

        public bool Equals(Volume? other)
        {
            return other != null && wKey == other.wKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(wKey);
        }
    }

    public class Issue : IEquatable<Issue?>
    {
        public Issue(ulong? journalKey, string? volumeName, string issueName)
        {
            wKey = WKey.Construct(journalKey?.ToString(), volumeName, issueName);
            IssueName = issueName;
        }

        public string wKey { get; }
        public string IssueName { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Issue);
        }

        public bool Equals(Issue? other)
        {
            return other != null &&
                   wKey == other.wKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(wKey);
        }
    }
}
