using System;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace title_cleaner
{
    class Program
    {
        private static string connStr = @"Server=localhost;Database=aminer;Integrated Security=True;";


        static void Main(string[] args)
        {
            Console.WriteLine("start");
            ReadData();
            Console.WriteLine("done");

            Console.ReadKey();
        }

        

        private static void ReadData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("article_sk", typeof(long)));
            dt.Columns.Add(new DataColumn("clean_article_title", typeof(string)));

            string query = "SELECT [id], [title] FROM [dbo].[paper] WHERE [title] IS NOT NULL;";
            using SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            using SqlCommand comm = new SqlCommand(query, conn);
            using SqlDataReader dr = comm.ExecuteReader();
            int i = 0;
            while (dr.Read())
            {
                i++;
                DataRow dataRow = dt.NewRow();
                dataRow["article_sk"] = dr.GetInt64(0);
                dataRow["clean_article_title"] = CleanString(dr.GetString(1));
                dt.Rows.Add(dataRow);
                if (i % 1000 == 0)
                {
                    FlushDataTable(dt);
                    dt.Clear();
                }
            }
            FlushDataTable(dt);
        }

        private static void FlushDataTable(DataTable dt)
        {
            Console.WriteLine("Flushing rows");

            using SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            using SqlBulkCopy bc = new SqlBulkCopy(conn)
            {
                DestinationTableName = "bus.clean_article_title"
            };
            bc.WriteToServer(dt);
        }

        private static string CleanString(string input)
        {
            return Regex.Replace(input.Trim().ToLower(), @"[^0-9a-zA-Z]+", "");
        }
    }
}
