using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Elsevier.ParseOutput
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");

            List<string> articleTags = new List<string>
            {
                "cid",
                "cover-date-start",
                "cover-date-text",
                "document-subtype",
                "document-type",
                "eid",
                "doi",
                "issuePii",
                "language",
                "pii",
                "srctitle",
                "suppl",
                "vol-first",
                "vol-iss-suppl-text",
                "issn",
                "issn-primary-formatted",
                "volRange",
                "titleString",
                "first-fp",
                "last-lp"
            };

            DataTable articles = new DataTable();
            articles.Columns.Add(new DataColumn("$_id", typeof(int)));
            foreach (var at in articleTags)
            {
                articles.Columns.Add(new DataColumn(at.Replace("-", "_"), typeof(string)));
            }

            DataTable datesDt = new DataTable();
            datesDt.Columns.Add(new DataColumn("$_id", typeof(int)));
            datesDt.Columns.Add(new DataColumn("type", typeof(string)));
            datesDt.Columns.Add(new DataColumn("value", typeof(string)));

            string dir = @"C:\Users\EwoudWesterbaan\Desktop\elsevier_output";
            foreach (string file in Directory.EnumerateFiles(dir))
            {
                Console.WriteLine($"Processing: {file}");


                //var file = @"C:\Users\EwoudWesterbaan\Desktop\elsevier_output\7328.json";

                int id = Int32.Parse(Path.GetFileNameWithoutExtension(file));
                Console.WriteLine($"id: {id}");
                JObject obj = JObject.Parse(File.ReadAllText(file));
                JObject article = (JObject)obj["article"];

                DataRow dr = articles.NewRow();
                dr["$_id"] = id;
                foreach (var t in articleTags)
                {
                    var value = article[t]?.Value<string>();
                    dr[t.Replace("-", "_")] = value;
                }

                articles.Rows.Add(dr);

                JObject dates = (JObject)article["dates"];
                foreach (var k in dates)
                {
                    if (k.Key.Equals("Revised", StringComparison.OrdinalIgnoreCase))
                    {
                        JArray revisedDates = (JArray)k.Value;
                        foreach (var rd in revisedDates)
                        {
                            DataRow dater = datesDt.NewRow();
                            dater["$_id"] = id;
                            dater["type"] = k.Key;
                            dater["value"] = rd.ToString();
                            datesDt.Rows.Add(dater);
                        }

                    }
                    else
                    {
                        DataRow dater = datesDt.NewRow();
                        dater["$_id"] = id;
                        dater["type"] = k.Key;
                        dater["value"] = k.Value;
                        datesDt.Rows.Add(dater);
                    }
                }

                // authors
                JObject authorsJson = (JObject)obj["authors"];
                (DataTable authors, DataTable refs, DataTable affiliations) = ReadAuthors(authorsJson, id);

                BulkCopy(authors, "dbo.authors");
                BulkCopy(refs, "dbo.[references]");
                BulkCopy(affiliations, "dbo.affiliations");
                BulkCopy(datesDt, "dbo.dates");
                BulkCopy(articles, "dbo.articles");
                authors.Clear();
                refs.Clear();
                affiliations.Clear();
                datesDt.Clear();
                articles.Clear();

            } // end for each file

            Console.WriteLine("Done.");

            Console.ReadKey();


        }

        private static readonly string connStr = @"Data Source=localhost; Initial Catalog=elsevier; Integrated Security=True;";

        private static void BulkCopy(DataTable dt, string targetTable)
        {
            using SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            using SqlBulkCopy bc = new SqlBulkCopy(conn)
            {
                DestinationTableName = targetTable
            };
            bc.WriteToServer(dt);
        }

        private static (DataTable authors, DataTable refs, DataTable affiliations) ReadAuthors(JObject authorsJson, int article_id)
        {
            DataTable dtAuthors = new DataTable();
            dtAuthors.Columns.Add(new DataColumn("$_id", typeof(int)) { DefaultValue = article_id });
            dtAuthors.Columns.Add(new DataColumn("article_author_id", typeof(string)));
            dtAuthors.Columns.Add(new DataColumn("property", typeof(string)));
            dtAuthors.Columns.Add(new DataColumn("value", typeof(string)));

            DataTable refsDt = new DataTable();
            refsDt.Columns.Add(new DataColumn("$_id", typeof(int)) { DefaultValue = article_id });
            refsDt.Columns.Add(new DataColumn("article_author_id", typeof(string)));
            refsDt.Columns.Add(new DataColumn("refid", typeof(string)));
            refsDt.Columns.Add(new DataColumn("article_cross_ref_id", typeof(string)));

            DataTable affDt = new DataTable();
            affDt.Columns.Add(new DataColumn("$_id", typeof(int)) { DefaultValue = article_id });
            affDt.Columns.Add(new DataColumn("affiliation_id", typeof(string)));
            affDt.Columns.Add(new DataColumn("property", typeof(string)));
            affDt.Columns.Add(new DataColumn("value", typeof(string)));

            JObject affiliations = (JObject)authorsJson["affiliations"];
            foreach(var affiliation in affiliations)
            {
                string affiliation_id = affiliation.Key;
                JObject affiliationJson = (JObject)affiliation.Value;
                JArray affiliationProperties = (JArray)affiliationJson["$$"];
                foreach (JObject property in affiliationProperties)
                {
                    string propLabel = property["#name"].Value<string>();
                    if (propLabel == "textfn")
                    {
                        DataRow dr = affDt.NewRow();
                        dr["affiliation_id"] = affiliation_id;
                        dr["property"] = propLabel;
                        string propValue = property["_"]?.Value<string>();
                        dr["value"] = propValue;
                        affDt.Rows.Add(dr);
                    }
                    else if (propLabel == "affiliation")
                    {
                        foreach (JObject p in (JArray)property["$$"])
                        {
                            DataRow dr = affDt.NewRow();
                            dr["affiliation_id"] = affiliation_id;
                            dr["property"] = p["#name"].Value<string>();
                            string propValue = p["_"]?.Value<string>();
                            dr["value"] = propValue;
                            affDt.Rows.Add(dr);
                        }
                    }
                }
            }

            JArray contentList = (JArray)authorsJson["content"];
            foreach (JObject content in contentList)
            {
                if (content["#name"].Value<string>() != "author-group") continue;
                JArray authorList = (JArray)content["$$"];
                foreach (JObject author in authorList)
                {
                    if (author["#name"].Value<string>() != "author") continue;
                    string article_author_id = author["$"]["id"].Value<string>();

                    string author_id = author["$"]["author-id"]?.Value<string>();
                    if (author_id != null)
                    {
                        DataRow dr = dtAuthors.NewRow();
                        dr["article_author_id"] = article_author_id;
                        dr["property"] = "author-id";
                        dr["value"] = author_id;
                        dtAuthors.Rows.Add(dr);
                    }

                    JArray authorProperties = (JArray)author["$$"];
                    foreach (JObject property in authorProperties)
                    {
                        if (property["#name"].Value<string>() == "cross-ref")
                        {
                            JObject cr = (JObject)property["$"];
                            DataRow dr = refsDt.NewRow();
                            dr["article_author_id"] = article_author_id;
                            dr["refid"] = cr["refid"].Value<string>();
                            dr["article_cross_ref_id"] = cr["id"]?.Value<string>();
                            refsDt.Rows.Add(dr);
                        }
                        else if (property["_"] == null) continue;
                        else
                        {
                            DataRow dr = dtAuthors.NewRow();
                            dr["article_author_id"] = article_author_id;
                            dr["property"] = property["#name"].Value<string>();
                            dr["value"] = property["_"]?.Value<string>();
                            dtAuthors.Rows.Add(dr);
                        }
                    }

                }
            } // End loop content
            return (dtAuthors, refsDt, affDt);
        }
    }
}
