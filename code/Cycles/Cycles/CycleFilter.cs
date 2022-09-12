namespace Cycles
{
    public class CycleFilter
    {
        public IReadOnlySet<NodeLabel> AllowedNodeLabels { get; }
        public IReadOnlySet<RelationshipLabel> AllowedRelationshipLabels { get; }
        public IReadOnlySet<PathPattern>? AllowedPathPatterns { get; }

        public static CycleFilter Default { get; } = new CycleFilter();

        private CycleFilter()
        {
            AllowedNodeLabels = new HashSet<NodeLabel>()
            {
                NodeLabel.Article,
                NodeLabel.Person,
                NodeLabel.Proceeding,
                NodeLabel.Issue,
                NodeLabel.Volume,
                NodeLabel.Journal,
                NodeLabel.Conference
            };

            AllowedRelationshipLabels = new HashSet<RelationshipLabel>()
            {
                RelationshipLabel.AuthorOf,
                RelationshipLabel.BelongsTo,
                RelationshipLabel.Cites,
                RelationshipLabel.EditorOf,
                RelationshipLabel.Of,
                RelationshipLabel.PcMemberOf
            };

            AllowedPathPatterns = null;
        }

        public CycleFilter(params PathPattern[] patterns)
        {
            var allowedPathPaterns = patterns.ToHashSet();
            var allowedNodeLabels = new HashSet<NodeLabel>();
            var allowedRelationshipLabels = new HashSet<RelationshipLabel>();

            foreach (var pattern in patterns)
            {
                var (NodeLabels, RelationshipLabels, _) = pattern.Extract();
                foreach (var nl in NodeLabels)
                    allowedNodeLabels.Add(nl);
                foreach (var rl in RelationshipLabels)
                    allowedRelationshipLabels.Add(rl);

                allowedPathPaterns.Add(pattern.Inverse());
            }

            AllowedNodeLabels = allowedNodeLabels;
            AllowedRelationshipLabels = allowedRelationshipLabels;
            AllowedPathPatterns = allowedPathPaterns;
        }

        public bool IsAllowedCycle(RecursiveCycle cycle)
        {
            if (AllowedPathPatterns is null)
                return true;

            return AllowedPathPatterns.Contains(cycle.Pattern);
        }

        public string FilterNodeCypher(string nodeName)
        {
            return $"({string.Join(" OR ", AllowedNodeLabels.Select(l => $"{nodeName}:{l.ToDB()}"))})";
        }

        public string FilterRelationCypher(string relationName)
        {
            return $"({string.Join(" OR ", AllowedRelationshipLabels.Select(l => $"{relationName}:{l.ToDB()}"))})";
        }
    }
}
