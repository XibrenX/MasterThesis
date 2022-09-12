// See https://aka.ms/new-console-template for more information

using Cycles;
using Cycles.Algorithms;
using Neo4j.Driver;
using System.Diagnostics;

int MAX_DEPTH = int.Parse(ReadConfig.Config["CYCLE_MAX_DEPTH"]);

Console.WriteLine("Please enter person name where to detect cycles for");
var personName = Console.ReadLine();

var cycleFilter = CycleFilter.Default;

var log = new StreamWriter(Console.OpenStandardOutput(), leaveOpen: true);

var person = new Person(personName);

var driver = GraphDatabase.Driver(ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(ReadConfig.Config["NEO4J_USERNAME"], ReadConfig.Config["NEO4J_PASSWORD"]));

var stopwatch = new Stopwatch();

stopwatch.Restart();

var groupedCycles = await person.GetCyclesAsync(driver, MAX_DEPTH, cycleFilter, new FullInfoOptions(), log);

stopwatch.Stop();

var count = groupedCycles.Values.SelectMany(kv => kv).LongCount();
log.Close();

foreach (var (pattern, cycles) in groupedCycles.OrderBy(kv => kv.Key))
{
    Console.WriteLine($"{pattern.ToShortString(true)}: {cycles.Count}");
}

Console.WriteLine("Time " + stopwatch.Elapsed);
Console.WriteLine("Total count " + groupedCycles.Sum((g) => g.Value.Sum(rc => rc.GetPathCounts())));


//
// Export to Excel
//

stopwatch.Restart();

ExportToExcel.ExportCyclesToExcel(groupedCycles, ReadConfig.Config["CYCLE_PERSONS"].Replace('/', '\\') + @$"\{personName}_{MAX_DEPTH}.xlsx");

stopwatch.Stop();

Console.WriteLine($"Exported {count} recursive cycles to Excel in {stopwatch.Elapsed.TotalSeconds:0.00} secs");

Console.WriteLine("Done!");