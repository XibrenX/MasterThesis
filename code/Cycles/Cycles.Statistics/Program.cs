// See https://aka.ms/new-console-template for more information
using Cycles;
using Cycles.Algorithms;
using Neo4j.Driver;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

int MAX_DEPTH = int.Parse(ReadConfig.Config["CYCLE_MAX_DEPTH"]);
const int BUFFER_SIZE = 1024 * 256;

var outputDir = ReadConfig.Config["CYCLE_STATS"].Replace('/', '\\');
var backupDir = outputDir += "_backup";

var log = new StreamWriter(Console.OpenStandardOutput(), leaveOpen: true);

var driver = GraphDatabase.Driver(ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(ReadConfig.Config["NEO4J_USERNAME"], ReadConfig.Config["NEO4J_PASSWORD"]));

var personsDoneFile = System.IO.Path.Combine(outputDir, "persons_done.txt");

var personsTodo = await ReadPersonsTodoAsync(driver);
var personsDone = await ReadPersonsDoneAsync(personsDoneFile);
personsTodo.RemoveAll(p => personsDone.Contains(p));

personsTodo.Sort();

var streams = new Dictionary<PathPattern, StreamWriter>();

var stopWatch = new Stopwatch();
stopWatch.Start();
var startTime = DateTime.Now;

using var writeBag = new BlockingCollection<(string person, Dictionary<PathPattern, (long recCount, long cycCount)> counts)>();

var cancelTokenSource = new CancellationTokenSource();

var processThread = Task.Run(() => RunProcessingThread(writeBag, personsTodo.Count, outputDir, backupDir, log, personsDoneFile));

var consoleThread = new Thread(() => RunConsoleThread(cancelTokenSource, log));
consoleThread.Start();

log.WriteLine("Start cycling");

try
{
    Parallel.ForEach(personsTodo, new ParallelOptions() { MaxDegreeOfParallelism = 8, CancellationToken = cancelTokenSource.Token }, () => GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "mtroot123")), (personName, s, driver) =>
    {
        if (s.IsStopped || s.ShouldExitCurrentIteration)
            return driver;

        var person = new Person(personName);
        var groupedCycles = person.GetCyclesAsync(driver, MAX_DEPTH, CycleFilter.Default, new LimitedInfoOptions(), null).GetAwaiter().GetResult();
        writeBag.Add((
            personName,
            new(
                groupedCycles.Select(kv => new KeyValuePair<PathPattern, (long recCount, long cycCount)>(
                    kv.Key,
                    (kv.Value.Count,
                        kv.Value.Sum(rc => rc.GetPathCounts()
                    ))
                ))
            )
        ), CancellationToken.None);

        return driver;
    }, (driver) => { driver.Dispose(); });
}
catch (TaskCanceledException) { }
catch (OperationCanceledException) { }

writeBag.CompleteAdding();

processThread.Wait();

log.WriteLine("Done");

log.Close();

Environment.Exit(0);

static async Task RunProcessingThread(BlockingCollection<(string person, Dictionary<PathPattern, (long recCount, long cycCount)> counts)> writeBag, int totalPersons, string outputDir, string backupDir, StreamWriter log, string personsDoneFile)
{
    var BACKUP_INTERVAL = TimeSpan.FromMinutes(30);

    using var personsDoneFileStream = new StreamWriter(personsDoneFile, true, System.Text.Encoding.UTF8, BUFFER_SIZE);

    var streams = new Dictionary<PathPattern, StreamWriter>();
    int personsDoneCount = 0;

    var stopWatch = new Stopwatch();
    stopWatch.Start();
    var startTime = DateTime.Now;
    var nextBackup = DateTime.Now + BACKUP_INTERVAL;

    foreach (var write in writeBag.GetConsumingEnumerable())
    {
        var (personName, counts) = write;

        personsDoneCount += 1;
        var avg = stopWatch.Elapsed / personsDoneCount;

        await Task.WhenAll(counts.Select((x) =>
        {
            var (pattern, (recCount, cycCount)) = x;
            if (!streams.TryGetValue(pattern, out var sw))
            {
                var groupFileName = System.IO.Path.Combine(outputDir, pattern.ToShortString(true) + ".csv");
                sw = new StreamWriter(groupFileName, true, System.Text.Encoding.UTF8, BUFFER_SIZE);
                streams.Add(pattern, sw);
            }
            return sw.WriteLineAsync($"{Escape(personName)},{recCount},{cycCount}");
        }).Concat(
            personsDoneFileStream.WriteLineAsync(personName)
        ));

        await log.WriteLineAsync($@"Progress: {personsDoneCount}/{totalPersons} ({(personsDoneCount * 100d) / totalPersons:00.00}%)
Avg: {avg.ToString(@"hh\:mm\:ss\.ffff")}
Expected done time: {startTime + (avg * totalPersons): dd-MM-yy HH:mm:ss}

");
        await log.FlushAsync();

        if (nextBackup < DateTime.Now) //Backup each interval
        {
            await log.WriteLineAsync($"## Backup performed");
            await Task.WhenAll(streams.Values.Select(sw => sw.FlushAsync()).Concat(personsDoneFileStream.FlushAsync()));
            CloneDirectory(outputDir, backupDir);
            nextBackup = DateTime.Now + BACKUP_INTERVAL;
        }
    }

    await log.WriteLineAsync("Cleaning up");
    await log.FlushAsync();

    while (streams.Count > 0)
    {
        streams.Remove(streams.Keys.First(), out var sw);
        await sw.FlushAsync();
        sw.Close();
    }

    await personsDoneFileStream.FlushAsync();
    personsDoneFileStream.Close();

    await log.WriteLineAsync("Done cleaning");
    await log.FlushAsync();
}

static void RunConsoleThread(CancellationTokenSource t, StreamWriter log)
{
    for(; ; )
    {
        var line = Console.ReadLine();
        if (line == "s")
        {
            log.WriteLine("## Stop requested");
            log.Flush();
            t.Cancel();
            return;
        }
        else if(line == null)
        {
            break;
        }
    }
}


static string? Escape(string? field)
{
    if (field is not null && (field.Contains(',') || field.Contains('\\') || field.Contains('"')))
    {
        return $"\"{field.Replace("\"", "\"\"").Replace("\\", "\\\\")}\"";
    }
    else
    {
        return field;
    }
}

static async Task<List<string>> ReadPersonsTodoAsync(IDriver driver)
{
    List<string> persons;

    using (var session = driver.AsyncSession())
    {
        var cursor = await session.RunAsync("MATCH (p:Person) RETURN p.dblpName");
        persons = await cursor.ToListAsync(r => r["p.dblpName"].As<string>());
    }

    return persons;
}

static async Task<HashSet<string>> ReadPersonsDoneAsync(string personsDoneFile)
{
    HashSet<string> persons = new();

    if (File.Exists(personsDoneFile))
    {
        using (var sr = new StreamReader(personsDoneFile))
        {
            while (!sr.EndOfStream)
            {
                var personName = await sr.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(personName))
                    persons.Add(personName);
            }
        }
    }

    return persons;
}

static void CloneDirectory(string root, string dest)
{
    Directory.CreateDirectory(dest);

    foreach (var file in Directory.GetFiles(root))
    {
        File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
    }

    foreach (var directory in Directory.GetDirectories(root))
    {
        string dirName = Path.GetFileName(directory);
        
        CloneDirectory(directory, Path.Combine(dest, dirName));
    }


}