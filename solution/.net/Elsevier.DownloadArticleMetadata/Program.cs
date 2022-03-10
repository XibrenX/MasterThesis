using System;
using System.Data.SqlClient;

namespace Elsevier.DownloadArticleMetadata
{
    class Program
    {
        private static readonly string savedir = @"<- output dir -> e.g. \Desktop\elsevier_output";

        private static readonly string torPath = @"<- path to tor browser ->\Tor Browser\Browser\firefox.exe";

        private static int _retryCount = 5;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting");

            JournalScraper js = new JournalScraper(savedir, torPath);
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
            while(dr.Read())
            {
                id = dr.GetInt64(0);
                title = dr.GetString(1);
            }
            return (id, title);
        }
    }
}
