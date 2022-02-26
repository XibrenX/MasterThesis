using HtmlAgilityPack;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Elsevier.Sketch
{
    class Program
    {

        private static string connStr = @"Data Source=localhost; Initial Catalog=elsevier; Integrated Security=True;";

        static void Main(string[] args)
        {
            Console.WriteLine("Started");
            Console.WriteLine("Getting journals");
            var journals = GetJournals();
            SaveDataTable(journals, "[load].[journals]");
            Console.WriteLine("Done");
            Console.ReadKey();
        }


        static DataTable GetJournals()
        {
            DataTable journals = new DataTable();
            journals.Columns.Add(new DataColumn("title", typeof(string)));
            journals.Columns.Add(new DataColumn("url", typeof(string)));

            int page = 1;
            while (true)
            {
                Console.WriteLine($"Page: {page}...");
                var url = $"https://www.elsevier.com/catalog?producttype=journals&page={page}";
                var web = new HtmlWeb();
                var doc = web.Load(url);
                var nodes = doc.DocumentNode.SelectNodes("//h5[@class='listing-products-info-text-title']/a");

                if (nodes == null) break;
                foreach (var x in nodes)
                {
                    Uri lnk = new Uri(x.GetAttributeValue("href", string.Empty));
                    string journalName = string.Empty;
                    if (lnk.Host == "www.elsevier.com")
                    {
                        journalName = lnk.Segments[2].TrimEnd('/');
                    }
                    else if (lnk.Host == "www.journals.elsevier.com")
                    {
                        journalName = lnk.Segments[1].TrimEnd('/');
                    }
                    DataRow dr = journals.NewRow();
                    dr["title"] = journalName;
                    dr["url"] = lnk.ToString();
                    journals.Rows.Add(dr);
                }
                page++;
            }
            return journals;
        }

        static void SaveDataTable(DataTable dt, string targetTable)
        {
            using SqlConnection connection = new SqlConnection(connStr);
            connection.Open();
            using SqlBulkCopy bc = new SqlBulkCopy(connection)
            {
                DestinationTableName = targetTable
            };
            foreach (DataColumn dc in dt.Columns)
            {
                bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping(dc.ColumnName, dc.ColumnName));
            }
            bc.WriteToServer(dt);
        }  

    }
}
