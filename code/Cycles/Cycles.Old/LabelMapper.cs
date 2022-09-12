namespace Cycles.Old
{
    public class LabelMapper
    {
        private static readonly Dictionary<string, char> typeToLabel = new Dictionary<string, char>
        {
            ["Person"] = 'P',
            ["DOI"] = 'D',
            ["Article"] = 'A',
            ["Issue"] = 'I',
            ["Journal"] = 'J',
            ["Orcid"] = 'O',
            ["Proceeding"] = 'R',
            ["Volume"] = 'V',

            ["author_of"] = 'a',
            ["belongs_to"] = 'b',
            ["editor_of"] = 'e',
            ["doi_of"] = 'd',
            ["of"] = 'f',
            ["orcid_of"] = 'o',
            ["cites"] = 'c'
        };

        private static readonly Dictionary<char, string> labelToType = typeToLabel.ToDictionary(x => x.Value, x => x.Key);

        public static char GetLabel(string type) => typeToLabel[type];

        public static string GetType(char label) => labelToType[label];
    }
}
