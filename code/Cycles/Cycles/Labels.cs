namespace Cycles
{
    public enum NodeLabel : ushort
    {
        Article,
        DOI,
        Issue,
        Journal,
        Orcid,
        Person,
        Proceeding,
        Volume,
        Conference
    }

    public enum RelationshipLabel : ushort
    {
        AuthorOf,
        BelongsTo,
        Cites,
        DoiOf,
        EditorOf,
        Of,
        OrcidOf,
        PcMemberOf
    }

    public static class LabelsExtensions
    {
        public static RelationshipLabel RelationshipLabelFromDB(string label) => Enum.Parse<RelationshipLabel>(label.ToPascalCase());

        public static string ToDB(this RelationshipLabel rl) => rl.ToString().to_snake_case();

        public static string ToDB(this NodeLabel nl) => nl.ToString();

        public static string ToShortString(this NodeLabel nl)
        {
            return nl switch
            {
                NodeLabel.Article => "A",
                NodeLabel.DOI => "D",
                NodeLabel.Issue => "I",
                NodeLabel.Journal => "J",
                NodeLabel.Orcid => "O",
                NodeLabel.Person => "P",
                NodeLabel.Proceeding => "R",
                NodeLabel.Volume => "V",
                NodeLabel.Conference => "C",
                _ => throw new NotImplementedException($"ShortString not implemented for label {nl}")
            };
        }

        public static string ToShortString(this RelationshipLabel rl)
        {
            return rl switch
            {
                RelationshipLabel.AuthorOf => "a",
                RelationshipLabel.BelongsTo => "b",
                RelationshipLabel.Cites => "c",
                RelationshipLabel.DoiOf => "d",
                RelationshipLabel.EditorOf => "e",
                RelationshipLabel.Of => "f",
                RelationshipLabel.OrcidOf => "o",
                RelationshipLabel.PcMemberOf => "p",
                _ => throw new NotImplementedException($"ShortString not implemented for label {rl}")
            };
        }
    }
}
