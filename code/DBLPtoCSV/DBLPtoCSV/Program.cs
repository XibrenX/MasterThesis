using DBLPtoCSV;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

ulong total_ee_count = 0;
ulong total_doi_regex_count = 0;

var doiRegex = new Regex(@"(10.\d{4,9}\/[-._;()/:%A-Z0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
var yearRegex = new Regex(@"^(20|19)\d\d", RegexOptions.Compiled);

var csvArticles = new CSVWriting("a", "dblpKey", "Title", "Year");
var csvArticles_Authors = new CSVWriting("aa", "dblpArticleKey", "dblpPersonName");
var csvArticles_Proceedings = new CSVWriting("ar", "dblpArticleKey", "dblpProceedingKey");
var csvArticles_Journals = new CSVWriting("aj", "dblpArticleKey", "wJournalKey");

var csvProceedings = new CSVWriting("r", "dblpKey", "Code", "Title", "Year", "ISBN", "Publisher");
var csvProceedings_Editors = new CSVWriting("re", "dblpProceedingKey", "dblpPersonName");

var csvBooks = new CSVWriting("b", "dblpKey", "Title", "Year", "ISBN", "Publisher");
var csvBooks_Authors = new CSVWriting("ba", "dblpKey", "dblpPersonName");
var csvBooks_Editors = new CSVWriting("be", "dblpKey", "dblpPersonName");

var csvWritings = new List<CSVWriting>() { csvArticles, csvArticles_Authors, csvArticles_Proceedings, csvArticles_Journals, csvProceedings, csvBooks, csvBooks_Authors, csvBooks_Editors };

var doi_articles = new Dictionary<string, List<string>>();

const int logInterval = 30;
var nextLogTime = DateTime.Now;

var persons = new HashSet<Person>();
var journals = new HashSet<Journal>();
var volumes = new HashSet<Volume>();
var issues = new HashSet<Issue>();
var wwws = new HashSet<WWW>();
var conferences = new HashSet<Conference>();
var series = new HashSet<Serie>();

var urlToKeyMapping = new Dictionary<string, string>();
using var doubleUrlLog = new StreamWriter(Path.Combine(ReadConfig.Config["NEO4J_IMPORT"], $"{ReadConfig.Config["DBLP_TO_CSV_RUNID"]}_double_url_log.txt"), false);
var doubleUrl = new HashSet<string>();

long i = 0;
foreach (var element in ReadElements(ReadConfig.Config["DBLP_XML_FILE"]))
{
    i += 1;
    if (nextLogTime < DateTime.Now)
    {
        nextLogTime = DateTime.Now.AddSeconds(logInterval);
        Console.WriteLine(string.Join(", ", csvWritings.Select(c => c.ShortProgress())));
    }

    //processUrl(element);

    switch (element.Name.LocalName)
    {
        case "article":
        case "inproceedings":
        //case "incollection":
            processArticle(element);
            break;
        case "proceedings":
            processProceeding(element);
            break;
        case "www":
            //processWWW(element);
            break;
        case "book":
            //processBook(element);
            break;
        case "phdthesis":
        case "mastersthesis":
            break;
        default:
            break;
    }
}

foreach (var writing in csvWritings)
{
    writing.Flush();
}

Console.WriteLine("Processing persons");

var csvPersons = new CSVWriting("p", "Name", "dblpName");
var csvPersons_Orcids = new CSVWriting("po", "dblpPersonName", "Orcid");

csvWritings.AddRange(new[] { csvPersons, csvPersons_Orcids });

foreach (var person in persons)
{
    if (nextLogTime < DateTime.Now)
    {
        nextLogTime = DateTime.Now.AddSeconds(logInterval);
        Console.WriteLine($"a: {csvPersons.TotalCount}/{persons.Count}");
    }

    csvPersons.WriteCSVLine(person.Name, person.dblpName);

    foreach (var orcid in person.Orcid)
    {
        csvPersons_Orcids.WriteCSVLine(person.dblpName, orcid);
    }
}

foreach (var writing in csvWritings)
{
    writing.Flush();
}

var csvJournals = new CSVWriting("j", "wKey", "Code", "Title", "Publisher");
csvWritings.Add(csvJournals);
foreach (var journal in journals)
{
    csvJournals.WriteCSVLine(journal.wKey.ToString(), journal.Code, journal.Title, journal.Publisher);
}

var csvVolumes = new CSVWriting("jv", "wKey");
csvWritings.Add(csvVolumes);
foreach (var volume in volumes)
{
    csvVolumes.WriteCSVLine(volume.wKey);
}

var csvIssues = new CSVWriting("jvi", "wKey");
csvWritings.Add(csvIssues);
foreach (var issue in issues)
{
    csvIssues.WriteCSVLine(issue.wKey);
}

var csvDoubleDoi = new CSVWriting("dd", "DOI", "Count");
var csvArticles_DOI = new CSVWriting("ad", "dblpArticleKey", "DOI");
csvWritings.AddRange(new[] { csvDoubleDoi, csvArticles_DOI });
foreach (var (doi, articles) in doi_articles)
{
    if(articles.Count > 1)
    {
        csvDoubleDoi.WriteCSVLine(doi, articles.Count.ToString());
    } else
    {
        csvArticles_DOI.WriteCSVLine(articles[0], doi);
    }
    
}

var csvConferences = new CSVWriting("c", "dblpKey", "Code");
csvWritings.Add(csvConferences);
foreach (var conference in conferences)
{
    csvConferences.WriteCSVLine(conference.DblpKey, conference.Code);
}

var csvSeries = new CSVWriting("s", "Name", "Url", "Key");
csvWritings.Add(csvSeries);
foreach(var serie in series)
{
    var key = serie.Url is null ? null : urlToKeyMapping.GetValueOrDefault(serie.Url);
    csvSeries.WriteCSVLine(serie.Name, serie.Url, key);
}

foreach (var csvWriting in csvWritings)
{
    csvWriting.Close();
}

using (var sw = new StreamWriter(Path.Combine(ReadConfig.Config["NEO4J_IMPORT"], $"{ReadConfig.Config["DBLP_TO_CSV_RUNID"]}_metrics.txt")))
{
    sw.Write(string.Join("\n\n", csvWritings.Select(c => c.LongProgress())));
    sw.Write($"\n\nDOI:\nEE: {total_ee_count}\nRegex: {total_doi_regex_count}\nDOIs: {csvArticles_DOI.TotalCount}\nUnique: {doi_articles.Count}\nDouble: {doi_articles.Where(kv => kv.Value.Count > 1).LongCount()}");
}

Console.WriteLine("Program ended");

//Code below copied from Ewoud IM9906\solution\.net\DblpLoader\Program.cs
IEnumerable<XElement> ReadElements(string filePath)
{
    XmlReaderSettings settings = new XmlReaderSettings()
    {
        DtdProcessing = DtdProcessing.Parse,
        ValidationType = ValidationType.DTD,
        XmlResolver = new XmlUrlResolver()
    };
    using (XmlReader reader = XmlReader.Create(filePath, settings))
    {
        reader.MoveToContent();
        while (!reader.EOF)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Depth == 1)
            {
                var el = XElement.ReadFrom(reader) as XElement;
                if (el != null)
                {
                    yield return el;
                }
            }
            else
            {
                reader.Read();
            }
        }
    }
    yield break;
}

void processArticle(XElement element)
{
    var dblpKey = element.Attribute("key")?.Value;
    var title = element.Element("title")?.Value;
    var year = element.Element("year")?.Value;
    var crossref = element.Element("crossref")?.Value;

    //filter tained data
    if (dblpKey is null || title is null || year is null)
        return;

    csvArticles.WriteCSVLine(dblpKey, title, year);

    //process authors
    foreach (var authorEl in element.Elements("author"))
    {
        var name = processPerson(authorEl);

        csvArticles_Authors.WriteCSVLine(dblpKey, name);
    }

    //process doi 
    ProcessDoi(element);

    
    if (crossref is not null)
    {
        //process ref to proceedng
        csvArticles_Proceedings.WriteCSVLine(dblpKey, crossref);
    }
    else
    {
        //process ref to journal
        var journal = element.Element("journal")?.Value;
        var volume = element.Element("volume")?.Value;
        var issue = element.Element("number")?.Value;

        ulong? journalKey = null;
        if (!string.IsNullOrWhiteSpace(journal))
        {
            string? code = dblpKey.StartsWith("journals/") ? dblpKey.Split('/')[1] : null;
            var publisher = element.Element("publisher")?.Value;
            var cJournal = new Journal(journal, publisher, code);
            journals.Add(cJournal);
            journalKey = cJournal.wKey;
        }

        string? volumeName = null;
        if (!string.IsNullOrWhiteSpace(volume))
        {
            var cVolume = new Volume(journalKey, volume);
            volumes.Add(cVolume);
            volumeName = cVolume.VolumeName;
        }

        string? issueName = null;
        if (!string.IsNullOrWhiteSpace(issue))
        {
            var cIssue = new Issue(journalKey, volumeName, issue);
            issues.Add(cIssue);
            issueName = cIssue.IssueName;
        }

        var wkey = WKey.Truncate(WKey.Construct(journalKey?.ToString(), volumeName, issueName));
        if (!string.IsNullOrEmpty(wkey))
            csvArticles_Journals.WriteCSVLine(dblpKey, wkey);
    }
}

void processProceeding(XElement element)
{
    var key = element.Attribute("key")?.Value;
    var title = element.Element("title")?.Value;
    var year = element.Element("year")?.Value;
    var isbn = element.Element("isbn")?.Value;
    var volume = element.Element("volume")?.Value;
    var publisher = element.Element("publisher")?.Value;
    var url = element.Element("url")?.Value;

    //processSeries(element);

    var keySplit = key?.Split('/');
    var code = key;
    if ((keySplit?.Length ?? 0) >= 3)
    {
        var match = yearRegex.Matches(keySplit[2]);
        if (match.Count == 1)
        {
            code = keySplit[1] + keySplit[2][2..];
        }
        else
        {
            code = keySplit[1] + keySplit[2];
        }
    }

    csvProceedings.WriteCSVLine(key, code, title, year, isbn, publisher);

    foreach (var editor in element.Elements("editor"))
    {
        var name = processPerson(editor);

        csvProceedings_Editors.WriteCSVLine(key, name);
    }

    var conference = Conference.GetConference(key);
    if (conference is not null)
        conferences.Add(conference);
}

void processBook(XElement element)
{
    var key = element.Attribute("key")?.Value;
    var title = element.Element("title")?.Value;
    var year = element.Element("year")?.Value;
    var isbn = element.Element("isbn")?.Value;
    //var volume = element.Element("volume")?.Value;
    var publisher = element.Element("publisher")?.Value;
    var url = element.Element("url")?.Value;

    processSeries(element);

    ProcessDoi(element);

    csvBooks.WriteCSVLine(key, title, year, isbn, publisher);

    foreach (var author in element.Elements("author"))
    {
        string name = processPerson(author);

        csvBooks_Authors.WriteCSVLine(key, name);
    }

    foreach (var editor in element.Elements("editor"))
    {
        string name = processPerson(editor);
        csvBooks_Editors.WriteCSVLine(key, name);
    }
}

void processWWW(XElement element)
{
    var key = element.Attribute("key")?.Value;
    if (key is null)
        return;

    var url = element.Element("url")?.Value;
    var note = element.Element("note")?.Value;

    var persons = new HashSet<Person>();

    foreach(var author in element.Elements("author"))
    {
        var name = author.Value;
        var orcid = author.Attribute("orcid")?.Value;

        var person = new Person(name, orcid);
        persons.Add(person);
    }

    var www = new WWW(key, persons, url, note);

    var crossref = element.Element("crossref")?.Value;
    if (crossref is not null)
    {
        wwws.Remove(www);
        www = www.TransposeKey(crossref);
    }
    wwws.Add(www);
}

void ProcessDoi(XElement element)
{
    var key = element.Attribute("key")?.Value;

    var dois = new HashSet<string>(); //Sometimes the same doi is added multiple times, probaly different URLs
    foreach (var ee in element.Elements("ee"))
    {
        total_ee_count += 1;
        var match = doiRegex.Match(ee.Value);
        if (match.Success)
        {
            total_doi_regex_count += 1;
            dois.Add(match.Captures[0].Value);
        }
    }

    foreach (var doi in dois)
    {
        if (!doi_articles.TryGetValue(doi, out var articles))
        {
            articles = new List<string>();
            doi_articles[doi] = articles;
        }
        articles.Add(key);
    }
}

string processPerson(XElement personElement)
{
    var name = personElement.Value;
    var orcid = personElement.Attribute("orcid")?.Value;

    var person = new Person(name, orcid);
    persons.Add(person);
    return name;
}

void processUrl(XElement element)
{
    var key = element.Attribute("key")?.Value;
    if (key is null)
        return;

    var dblpUrls = element.Elements("url").Select(u => u.Value).Where(u => !u.Contains("://")).ToList();
    if(dblpUrls.Count > 1)
    {
        //Did not happen yet
    }
    else if(dblpUrls.Count == 1)
    {
        var url = dblpUrls[0];

        if(doubleUrl.Contains(url))
        {
            doubleUrlLog.WriteLine($"Sec conflict: {url}, {key}");
            return;
        }

        if (urlToKeyMapping.TryGetValue(url, out var otherKey) )
        {
            doubleUrl.Add(url);
            doubleUrlLog.WriteLine($"Url conflict: {url}, {key}, {otherKey}");
            urlToKeyMapping.Remove(url);
        } 
        else
        {
            urlToKeyMapping.Add(url, key);
        }
    }
}

void processSeries(XElement element)
{
    //var key = element.Attribute("key")?.Value;

    //var series_name = element.Element("series")?.Value;
    //var series_href = element.Element("series")?.Attribute("href")?.Value;

    //if(series_name is not null)
    //{
    //    var s = new Serie(series_name, series_href);
    //    series.Add(s);
    //    //TODO link serie
    //}
}

