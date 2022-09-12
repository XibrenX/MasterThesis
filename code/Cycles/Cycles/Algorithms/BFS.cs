using Neo4j.Driver;
using System.Diagnostics;

namespace Cycles.Algorithms
{
    public static class BFS
    {
        private const string NODE = "n";
        private const string RELATIONSHIP = "e";
        private const string STARTNODE = "s";
        private const string STARTNODEID = "startNodeId";

        public static void FilterNodesByLabels(Stopwatch stopwatch, Dictionary<long, Node> nodes_dir, CycleFilter p)
        {
            stopwatch.Restart();

            foreach (var node in nodes_dir.Values.ToList())
            {
                if (!p.AllowedNodeLabels.Contains(node.Label))
                {
                    foreach (var relation in node.Relations)
                    {
                        relation.Detach();
                    }
                    nodes_dir.Remove(node.Id);
                }
            }

            stopwatch.Stop();
        }

        public static async Task GetNodesAndRelations(IAsyncSession session, int depth, long startNodeId, Dictionary<long, Node> nodes_dir, Stopwatch stopwatch, CycleFilter filter, InfoOptions infoOptions)
        {
            var parameters = new Dictionary<string, object>() { [STARTNODEID] = startNodeId };

            infoOptions.SetNames(NODE, RELATIONSHIP);

            stopwatch.Start();
            {
                var query =
@$"
MATCH ({STARTNODE})-[*0..{depth - 1}]-()-[{RELATIONSHIP}]-({NODE}) 
WHERE ID({STARTNODE}) = ${STARTNODEID} 
    AND {filter.FilterNodeCypher(NODE)} 
    AND {filter.FilterRelationCypher(RELATIONSHIP)} 
RETURN {infoOptions.GetCyper()}
";

                var cursor = await session.RunAsync(query, parameters);

                await cursor.ForEachAsync((r) =>
                {
                    infoOptions.Construct(r, nodes_dir);
                });
            }

            stopwatch.Stop();
        }

        public static async Task<ulong> LoadRelationsCount(IDriver driver, Stopwatch stopwatch, Dictionary<long, Node> nodes_dir)
        {
            stopwatch.Restart();

            ulong relationsCount = 0;
            using (var session = driver.AsyncSession())
            {
                var result = await session.RunAsync("MATCH (n)-[r]-() WHERE ID(n) IN $nodeIds RETURN ID(n), type(r), startNode(r) = n, count(r)", new Dictionary<string, object>() { { "nodeIds", nodes_dir.Values.Select(n => n.Id) } });
                await result.ForEachAsync((record) =>
                {
                    relationsCount += 1;

                    if (nodes_dir.TryGetValue(record["ID(n)"].As<long>(), out var node))
                    {
                        node.AddRelationsCount(
                            Enum.Parse<RelationshipLabel>(record["type(r)"].As<string>().ToPascalCase()),
                            record["startNode(r) = n"].As<bool>() ? RelationshipDirection.Out : RelationshipDirection.In,
                            record["count(r)"].As<long>()
                        );
                    }
                });
            }

            stopwatch.Stop();
            return relationsCount;
        }

        public static void RemoveDeadEnds(Stopwatch stopwatch, Dictionary<long, Node> nodes_dir)
        {
            stopwatch.Restart();

            var nodesCopy = new List<Node>(nodes_dir.Values);
            foreach (var node in nodesCopy)
            {
                if (nodes_dir.ContainsKey(node.Id))
                    TryRemoveDeadEnd(node, nodes_dir);
            }

            stopwatch.Stop();
        }

        public static void RemoveTooDeepNodes(int depth, Stopwatch stopwatch, Dictionary<long, Node> nodes_dir)
        {
            stopwatch.Restart();

            var nodesToRemove = new List<long>(nodes_dir.Count);

            foreach (var node in nodes_dir.Values)
            {
                if (node.BFSInfo.Distance > depth)
                {
                    nodesToRemove.Add(node.Id);
                }
            }

            foreach (var nodeId in nodesToRemove)
            {
                if (nodes_dir.Remove(nodeId, out var node))
                {
                    foreach (var relation in node.Relations)
                    {
                        relation.Detach();
                    }
                }
            }

            stopwatch.Stop();
        }

        public static void TryRemoveDeadEnd(Node node, Dictionary<long, Node> nodes_dir)
        {
            if (node.Relations.Count == 0) //should not be possible
            {
                nodes_dir.Remove(node.Id);
            }

            if (node.Relations.Count == 1) //Dead end
            {
                var relation = node.Relations.First();
                var otherNode = relation.GetOtherNode(node);
                relation.Detach();
                nodes_dir.Remove(node.Id);
                TryRemoveDeadEnd(otherNode, nodes_dir);
            }
        }
    }
}