using Neo4j.Driver;
using System.Diagnostics;

namespace Neo4j.Interface
{
    /// <summary>
    /// Neo4j has issues with deleting large amounts of nodes and relationships, this program scales up to delete as many as quickly as possible, while respecting Neo4j performance and batch size
    /// </summary>
    public class RemoveLabel
    {
        public static async Task RunNoLabel(ulong idealWaitTimePerBatchInSeconds)
        {
            var driver = GraphDatabase.Driver(ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(ReadConfig.Config["NEO4J_USERNAME"], ReadConfig.Config["NEO4J_PASSWORD"]));
            var session = driver.AsyncSession();

            var cursor = await session.RunAsync($"MATCH (n) WHERE size(labels(n)) = 0 RETURN count(n) as count");
            var countToDelete = Convert.ToUInt64((await cursor.SingleAsync())["count"]);

            ulong currentDeleteLimit = 100;
            ulong totalDeleted = 0;

            var stopwatch = new Stopwatch();

            do
            {
                Console.WriteLine($"Deleting NoLabel: {totalDeleted}/{countToDelete} with steps of {currentDeleteLimit}");
                stopwatch.Restart();
                try
                {
                    cursor = await session.RunAsync($"MATCH (n) WHERE size(labels(n)) = 0 WITH n LIMIT {currentDeleteLimit} DETACH DELETE n RETURN count(*) as count");
                    var currentCount = Convert.ToUInt64((await cursor.SingleAsync())["count"]);
                    stopwatch.Stop();
                    totalDeleted += currentCount;
                    Console.WriteLine($"{currentCount} deleted in {stopwatch.Elapsed.TotalSeconds:0.0} seconds");
                }
                catch (TransientException e)
                {
                    stopwatch.Stop();
                    if (e.Code != "Neo.TransientError.General.OutOfMemoryError")
                        throw e;
                    idealWaitTimePerBatchInSeconds -= 1;
                    Console.WriteLine($"Out of memory in {stopwatch.Elapsed.TotalSeconds:0.0} seconds, trying again with lower amount");
                }

                currentDeleteLimit = Math.Max(100, currentDeleteLimit * idealWaitTimePerBatchInSeconds / (ulong)Math.Max(1, stopwatch.Elapsed.TotalSeconds));
            } while (countToDelete > totalDeleted);

            Console.WriteLine($"Deleted all");
        }

        public static async Task Run(string label, ulong idealWaitTimePerBatchInSeconds)
        {
            var driver = GraphDatabase.Driver(ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(ReadConfig.Config["NEO4J_USERNAME"], ReadConfig.Config["NEO4J_PASSWORD"]));
            var session = driver.AsyncSession();

            var cursor = await session.RunAsync($"MATCH (n:{label}) RETURN count(n) as count");
            var countToDelete = Convert.ToUInt64((await cursor.SingleAsync())["count"]);

            ulong currentDeleteLimit = 100;
            ulong totalDeleted = 0;

            var stopwatch = new Stopwatch();

            do
            {
                Console.WriteLine($"Deleting {label}: {totalDeleted}/{countToDelete} with steps of {currentDeleteLimit}");
                stopwatch.Restart();
                try
                {
                    cursor = await session.RunAsync($"MATCH (n:{label}) WITH n LIMIT {currentDeleteLimit} DETACH DELETE n RETURN count(*) as count");
                    var currentCount = Convert.ToUInt64((await cursor.SingleAsync())["count"]);
                    stopwatch.Stop();
                    totalDeleted += currentCount;
                    Console.WriteLine($"{currentCount} deleted in {stopwatch.Elapsed.TotalSeconds:0.0} seconds");
                }
                catch (TransientException e)
                {
                    stopwatch.Stop();
                    if (e.Code != "Neo.TransientError.General.OutOfMemoryError")
                        throw e;
                    idealWaitTimePerBatchInSeconds -= 1;
                    Console.WriteLine($"Out of memory in {stopwatch.Elapsed.TotalSeconds:0.0} seconds, trying again with lower amount");
                }

                currentDeleteLimit = Math.Max(100, currentDeleteLimit * idealWaitTimePerBatchInSeconds / (ulong)Math.Max(1, stopwatch.Elapsed.TotalSeconds));
            } while (countToDelete > totalDeleted);

            Console.WriteLine($"Deleted all");
        }

        public static async Task RunRelationship(string label, ulong idealWaitTimePerBatchInSeconds)
        {
            var driver = GraphDatabase.Driver(ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(ReadConfig.Config["NEO4J_USERNAME"], ReadConfig.Config["NEO4J_PASSWORD"]));
            var session = driver.AsyncSession();

            var cursor = await session.RunAsync($"MATCH ()-[r:{label}]->() RETURN count(r) as count");
            var countToDelete = Convert.ToUInt64((await cursor.SingleAsync())["count"]);

            ulong currentDeleteLimit = 100;
            ulong totalDeleted = 0;

            var stopwatch = new Stopwatch();

            do
            {
                Console.WriteLine($"Deleting {label}: {totalDeleted}/{countToDelete} with steps of {currentDeleteLimit}");
                stopwatch.Restart();
                try
                {
                    cursor = await session.RunAsync($"MATCH ()-[r:{label}]->() WITH r LIMIT {currentDeleteLimit} DELETE r RETURN count(*) as count");
                    var currentCount = Convert.ToUInt64((await cursor.SingleAsync())["count"]);
                    stopwatch.Stop();
                    totalDeleted += currentCount;
                    Console.WriteLine($"{currentCount} deleted in {stopwatch.Elapsed.TotalSeconds:0.0} seconds");
                }
                catch (TransientException e)
                {
                    stopwatch.Stop();
                    if (e.Code != "Neo.TransientError.General.OutOfMemoryError")
                        throw e;
                    idealWaitTimePerBatchInSeconds -= 1;
                    Console.WriteLine($"Out of memory in {stopwatch.Elapsed.TotalSeconds:0.0} seconds, trying again with lower amount");
                }

                currentDeleteLimit = Math.Max(100, currentDeleteLimit * idealWaitTimePerBatchInSeconds / (ulong)Math.Max(1, stopwatch.Elapsed.TotalSeconds));
            } while (countToDelete > totalDeleted);

            Console.WriteLine($"Deleted all");
        }
    }
}
