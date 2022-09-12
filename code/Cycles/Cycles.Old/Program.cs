// See https://aka.ms/new-console-template for more information
using Cycles.Old;
using Neo4j.Driver;
using System.Collections.Concurrent;

int MAX_RELATIONS = int.Parse(ReadConfig.Config["CYCLE_MAX_DEPTH"]) * 2;

Console.WriteLine("Please enter person name where to detect cycles for");
var personName = Console.ReadLine();

ulong rawCycleCount = 0;
var cyclesHash = new HashSet<Cycle>();
var mergedCycles = new CreateDictionary<string, List<MergedCycle>>();

var driver = GraphDatabase.Driver(ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(ReadConfig.Config["NEO4J_USERNAME"], ReadConfig.Config["NEO4J_PASSWORD"]));
var session = driver.AsyncSession();

var cache = new NodeRelationsCache(driver.AsyncSession()); // Different session for nodes cache

var addWeightsQueue = new BlockingCollection<Cycle>();
var addMergedCyclesQueue = new BlockingCollection<Cycle>();

var cursor = await session.RunAsync($"MATCH p=(a:Person)-[*2..{MAX_RELATIONS}]-(a) WHERE a.dblpName = $personName RETURN p", new Dictionary<string, object>() { ["personName"] = personName });

var consoleLock = new Object();
ulong weightsProcessed = 0;

var nodesManager = new NodesManager();

Task.WaitAll(
    cursor.ForEachAsync((r) =>
    {
        rawCycleCount += 1;
        var cycle = Cycle.Create(r.Values["p"] as IPath, nodesManager);
        if(cyclesHash.Add(cycle))
            addWeightsQueue.Add(cycle);
        lock (consoleLock)
        {
            Console.CursorTop = 0;
            Console.WriteLine($"Cycles: {cyclesHash.Count}/{rawCycleCount}");
        }
    }).ContinueWith((_) => {
        addWeightsQueue.CompleteAdding();
        lock (consoleLock)
        {
            Console.CursorTop = 0;
            Console.WriteLine($"Cycles: {cyclesHash.Count}/{rawCycleCount} (done)");
        }
    }) 
,
    Task.Run(async () => {
        foreach (var cycle in addWeightsQueue.GetConsumingEnumerable())
        {
            await cycle.AddWeights(cache);
            addMergedCyclesQueue.Add(cycle);
            weightsProcessed += 1;
            lock (consoleLock)
            {
                Console.CursorTop = 1;
                Console.WriteLine($"Weight cycles: {weightsProcessed} ({addWeightsQueue.Count})");
            }
        }
    }).ContinueWith((_) => {
        addMergedCyclesQueue.CompleteAdding();
        lock (consoleLock)
        {
            Console.CursorTop = 1;
            Console.WriteLine($"Weight cycles: {weightsProcessed} (done)");
        }
    })
,
    Task.Run(() => {
        foreach (var cycle in addMergedCyclesQueue.GetConsumingEnumerable())
        {
            var mc = new MergedCycle(cycle);
            var mcs = mergedCycles[mc.Label];
            mcs.Add(mc);
            DoMergeCycle(mcs.Count - 1, mcs, 'A');
            lock (consoleLock)
            {
                Console.CursorTop = 2;
                Console.WriteLine($"Merged cycles: {mergedCycles.Sum(kv => kv.Value.Count)} ({addMergedCyclesQueue.Count})");
            }
        }
    })
);

Console.CursorTop = 3;

var cycles = cyclesHash.ToList();

cycles.Sort((c1, c2) => {
    var lengthComp = c1.Label.Length.CompareTo(c2.Label.Length);
    return lengthComp == 0 ? c1.Label.CompareTo(c2.Label) : lengthComp;
});

foreach (var cycleCount in cycles.GroupBy(c => c.Relationships.Count)) {
    Console.WriteLine($"{cycleCount.Key}: {cycleCount.Count()}");
}
Console.WriteLine();

foreach (var cycleType in cycles.GroupBy(c => c.Label)) {
    Console.WriteLine($"{cycleType.Key}: {cycleType.Count()}");
}

ExportToExcel.ExportCyclesToExcel(mergedCycles.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value), ReadConfig.Config["CYCLE_PERSONS"].Replace('/', '\\') + @$"\old_{personName}_{MAX_RELATIONS}.xls");

void DoMergeCycle(int index, List<MergedCycle> list, char currentMergeLabel)
{
    var mc = list[index];
    if (!mc.Label.Contains(currentMergeLabel))
        return;

    for(int i = 0; i < list.Count; i++)
    {
        if (i == index)
            continue;
        if (mc.TryMerge(list[i], currentMergeLabel))
        {
            list.RemoveAt(index);
            if (index < i)
                i -= 1;
            DoMergeCycle(i, list, currentMergeLabel);
            return;
        }
    }
}