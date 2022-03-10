using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Helper;

namespace Elsevier.DownloadArticleMetadata
{
    class Program
    {
        private static int _retryCount = 5;

        private static readonly string propertiesFile = "../../../../../config";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting");

            Dictionary<string, string> properties = PropertiesReader.ReadProperties(propertiesFile);
            string savedir = Path.Combine(properties["RAW_DATA"], properties["ELSEVIER_ARTICLE_JSON_SUBDIR"]);

            JournalScraper js = new JournalScraper(savedir);
            js.RefreshBrowser();

            while (true)
            {
                (long id, string title) = GetJournal();
                if (id == 0) break;
                int retryCounter = 0;
                Console.WriteLine($"Start processing journal {id} ({title})");
                bool succeeded = false;
                while (!succeeded && retryCounter < _retryCount)
                {
                    retryCounter++;
                    
                    try
                    {
                        js.GetJournalData(id, title);
                        Console.WriteLine($"Attempt {retryCounter} succeeded");
                        succeeded = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine($"Attempt {retryCounter} failed");
                        js.RefreshBrowser();
                    }
                }
                if (!succeeded)
                    Console.WriteLine($"FAILED processing journal {id} ({title})");
                else
                    Console.WriteLine($"SUCCEEDED processing journal {id} ({title})");
            }
            Console.WriteLine("Done");
        }


        private static readonly string connStr = @"Data Source=localhost; Initial Catalog=elsevier; Integrated Security=True;";

        //static (long, string) GetJournal()
        //{
        //    string sql = "UPDATE [elsevier].[load].[journals] " +
        //                 "SET [status] = 'RUNNING' " +
        //                 "OUTPUT INSERTED.[$_id], INSERTED.[title] " +
        //                 "WHERE [$_id] = (" +
        //                 "SELECT MIN([$_id]) " +
        //                 "FROM " +
        //                 "elsevier.dbo.elsevier_workload)";
        //    using SqlConnection connection = new SqlConnection(connStr);
        //    connection.Open();
        //    using SqlCommand command = new SqlCommand(sql, connection);
        //    using SqlDataReader dr = command.ExecuteReader();
        //    long id = 0;
        //    string title = string.Empty;
        //    while(dr.Read())
        //    {
        //        id = dr.GetInt64(0);
        //        title = dr.GetString(1);
        //    }
        //    return (id, title);
        //}

        static (long, string) GetJournal()
        {
            string sql = "UPDATE [elsevier].[load].[journals] " +
                         "SET [status] = 'RUNNING' " +
                         "OUTPUT INSERTED.[$_id], INSERTED.[title] " +
                         "WHERE [$_id] = (" +
                         "SELECT MIN([$_id]) " +
                         "FROM " +
                         "elsevier.dbo.elsevier_workload)";
            using SqlConnection connection = new SqlConnection(connStr);
            connection.Open();
            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader dr = command.ExecuteReader();
            long id = 0;
            string title = string.Empty;
            while (dr.Read())
            {
                id = dr.GetInt64(0);
                title = dr.GetString(1);
            }
            return (id, title);
        }
    }
}
