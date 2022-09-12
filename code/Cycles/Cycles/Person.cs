using Cycles.Algorithms;
using Neo4j.Driver;
using System.Diagnostics;

namespace Cycles
{
    public class Person
    {
        public Node? Node { get; private set; }
        public string DblpName { get; }

        private const string PERSON = "p";

        public Person(string dblpName)
        {
            DblpName = dblpName;
        }

        private async Task SetNode(IAsyncSession session)
        {
            var parameters = new Dictionary<string, object>() { ["personName"] = DblpName };

            var cursor = await session.RunAsync($"MATCH ({PERSON}:Person) WHERE {PERSON}.dblpName = $personName RETURN {PERSON}", parameters);
            var person = (await cursor.SingleAsync())[PERSON] as INode;
            Node = new Node(person);
            Node.BFSInfo.SetDistance(0);
        }

        public async Task<Dictionary<PathPattern, List<RecursiveCycle>>> GetCyclesAsync(IDriver driver, int depth, CycleFilter filter, InfoOptions infoOptions, StreamWriter? log)
        {
            var stopwatch = new Stopwatch();
            var nodes_dir = new Dictionary<long, Node>();

            using (var session = driver.AsyncSession(builder =>
            {
                builder.WithDefaultAccessMode(AccessMode.Read);
                builder.WithFetchSize(2000);
            }))
            {
                if (log is not null)
                {
                    await log.WriteLineAsync($"Detecting cycles for {DblpName}");
                    await log.FlushAsync();
                }

                //
                // Get Person
                //
                await SetNode(session);
                nodes_dir.Add(Node.Id, Node);

                //
                // Get Nodes and Relations Recursive
                //
                await BFS.GetNodesAndRelations(session, depth, Node.Id, nodes_dir, stopwatch, filter, infoOptions);
            }

            var initialNodesCount = nodes_dir.Count;
            if (log is not null)
                await log.WriteLineAsync($"Recieved {initialNodesCount} nodes count in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

            //
            // Filter nodes by labels
            //
            //BFS.FilterNodesByLabels(stopwatch, nodes_dir, filter);

            var afterNodeLabelFilterCount = nodes_dir.Count;
            //log.WriteLine($"Nodes passed label filter: {afterNodeLabelFilterCount} ({afterNodeLabelFilterCount - initialNodesCount}) in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

            //
            // Remove too deep nodes
            //
            BFS.RemoveTooDeepNodes(depth, stopwatch, nodes_dir);

            var afterToDeepFilterCount = nodes_dir.Count;
            if (log is not null)
                await log.WriteLineAsync($"In allowed depth nodes: {afterToDeepFilterCount} ({afterToDeepFilterCount - afterNodeLabelFilterCount}) in {stopwatch.Elapsed.TotalSeconds:0.00} secs");


            //
            // Remove Dead Ends 
            //
            BFS.RemoveDeadEnds(stopwatch, nodes_dir);

            var afterDeadEndFilterNodesCount = nodes_dir.Count;
            if (log is not null)
            {
                await log.WriteLineAsync($"Non dead end nodes: {afterDeadEndFilterNodesCount} ({afterDeadEndFilterNodesCount - afterToDeepFilterCount}) in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

                await log.WriteLineAsync($"Distance of nodes: {string.Join(", ", nodes_dir.Values.GroupBy(v => v.BFSInfo.Distance).Select(kv => (kv.Key, kv.LongCount())).OrderBy(kv => kv.Key).Select(kv => kv.Item2))}");
            }

            //
            // Load relations count
            //
            //ulong relationsCount = await BFS.LoadRelationsCount(driver, stopwatch, nodes_dir);

            //await log.WriteLineAsync($"Loaded relations {relationsCount} count for {afterDeadEndFilterNodesCount} nodes in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

            //
            // Perform BFS with cycle detection
            //
            var cycles = CycleDetection.PerformBFSCycleDetection(stopwatch, nodes_dir, filter);
            if (log is not null)
                await log.WriteLineAsync($"Used BFS to detect cycles. {cycles.Count} recursive cycles found in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

            //
            // Order cycles
            //
            CycleDetection.OrderCycles(stopwatch, cycles);
            if (log is not null)
                await log.WriteLineAsync($"Orderned {cycles.Count} recursive cycles in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

            //
            // Group cycles
            //
            var groupedCycles = CycleDetection.GroupCycles(stopwatch, cycles);
            if (log is not null)
                await log.WriteLineAsync($"Grouped {cycles.Count} recursive cycles in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

            return groupedCycles;
        }
    }
}
