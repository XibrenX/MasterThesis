// See https://aka.ms/new-console-template for more information


using CoAuthors;
using Neo4j.Driver;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

var driver = GraphDatabase.Driver(Cycles.ReadConfig.Config["NEO4J_SERVER"], AuthTokens.Basic(Cycles.ReadConfig.Config["NEO4J_USERNAME"], Cycles.ReadConfig.Config["NEO4J_PASSWORD"]));
var session = driver.AsyncSession();

var persons = new Dictionary<string, Person>();

var cursor = await session.RunAsync("MATCH (p1:Person)-[:author_of]->(:Article)<-[:author_of]-(p2:Person) RETURN p1.dblpName, p2.dblpName");
await cursor.ForEachAsync((r) =>
{
    var p1Name = r["p1.dblpName"].As<string>();
    if (!persons.TryGetValue(p1Name, out var p1)) {
        p1 = new Person(p1Name);
        persons.Add(p1Name, p1);
    }

    var p2Name = r["p2.dblpName"].As<string>();
    if (!persons.TryGetValue(p2Name, out var p2)) {
        p2 = new Person(p2Name);
        persons.Add(p2Name, p2);
    }

    CoAuthors.CoAuthors.Create(p1, p2);
});

await session.CloseAsync();
await driver.CloseAsync();

var workbook = new XSSFWorkbook();
ISheet sheet = workbook.CreateSheet();
int rowNumber = 0;

foreach(var ca in CoAuthors.CoAuthors.CAs)
{
    if (rowNumber > 1000000)
    {
        sheet = workbook.CreateSheet();
        rowNumber = 0;
    }

    var row = sheet.CreateRow(rowNumber++);
    row.CreateCell(0).SetCellValue(ca.PersonA.Name);
    row.CreateCell(1).SetCellValue(ca.Count);
    row.CreateCell(2).SetCellValue(ca.PersonB.Name);
}

using (var stream = new FileStream(Cycles.ReadConfig.Config["CYCLE_PERSONS"].Replace('/', '\\') + @"\coauthors.xlsx", FileMode.Create, FileAccess.Write))
{
    workbook.Write(stream);
}