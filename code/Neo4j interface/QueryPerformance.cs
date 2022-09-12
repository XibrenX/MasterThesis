using Neo4j.Driver;
using System.Diagnostics;

namespace Neo4j.Interface
{
    public class QueryPerformance
    {
        private static readonly int tryCount = 10;
        public static async Task Run(params Query[] queries)
        {
            using var log = new StreamWriter("log.txt", true);
            await log.WriteLineAsync($"Session {DateTime.Now:dd-MM-yyyy HH:mm}");

            var driver = GraphDatabase.Driver(ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(ReadConfig.Config["NEO4J_USERNAME"], ReadConfig.Config["NEO4J_PASSWORD"]));
            var session = driver.AsyncSession(b => { b.WithDefaultAccessMode(AccessMode.Read).WithFetchSize(2000); });
            var stopwatch = new Stopwatch();

            foreach (var query in queries)
            {
                ulong? count = null;
                Console.WriteLine(query.Text);
                await log.WriteLineAsync(query.Text);

                stopwatch.Reset();
                var startTime = DateTime.Now;

                for (int i = 0; i < tryCount; i++)
                {
                    Console.Write($"\rRunning {i}/{tryCount}");
                    if (i > 0)
                        Console.Write($" expected end: {startTime + stopwatch.Elapsed / i * tryCount:dd-MM-yyyy HH:mm}     ");

                    ulong thisCount = 0;
                    stopwatch.Start();
                    var cursor = await session.RunAsync(query);
                    await cursor.ForEachAsync((x) =>
                    {
                        thisCount += 1;
                    });
                    var result = await cursor.ConsumeAsync();
                    stopwatch.Stop();

                    if (count.HasValue)
                    {
                        if (count != thisCount)
                        {
                            Console.WriteLine($"  Count inconsitency last {count} this {thisCount}");
                            await log.WriteLineAsync($"  Count inconsitency last {count} this {thisCount}");
                        }
                    }
                    else
                    {
                        count = thisCount;
                    }
                }

                Console.WriteLine($"  SW: {stopwatch.Elapsed / tryCount}");
                //Console.WriteLine($"  AF: {result.ResultAvailableAfter}");
                //Console.WriteLine($"  CF: {result.ResultConsumedAfter}");
                Console.WriteLine($"  Co: {count}");

                await log.WriteLineAsync($"  SW: {stopwatch.Elapsed / tryCount}");
                await log.WriteLineAsync($"  Co: {count}");
                await log.WriteLineAsync();
                await log.FlushAsync();
            }
        }
    }
}
