using Neo4j.Driver;

namespace Cycles.Old
{
    public class DisplayMapper
    {
        public static string GetDisplay(INode node)
        {
            return node.Labels[0] switch
            {
                "Article" => $"{node["Year"].As<int>() % 100} {node["Title"].As<string>().Truncate(15)}",
                "DOI" => node["d"].As<string>(),
                "Issue" => node["wKey"].As<string>(),
                "Journal" => node["Code"].As<string>(),
                "Orcid" => node["o"].As<string>(),
                "Person" => node["Name"].As<string>(),
                "Proceeding" => ProcessProceeding(node),
                "Volume" => node["wKey"].As<string>(),
                _ => string.Empty
            };
        }

        private static string ProcessProceeding(INode node)
        {
            var key = node["dblpKey"].As<string>();
            if (key.StartsWith("conf/"))
            {
                key = key.Substring("conf/".Length);
            }
            return key;
        }
    }
}
