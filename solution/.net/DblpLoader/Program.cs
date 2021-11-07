using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Helper;
using Database;

namespace dblp.loader
{
    class Program
    {
        private static string schema = "dblp_dump";
        private static string propertiesFile = "solution/config";


        [Obsolete]
        static void Main(string[] args)
        {
            Console.WriteLine("Program started");

            Dictionary<string, string> properties = PropertiesReader.ReadProperties(propertiesFile);

            string user = properties["POSTGRES_USER"];
            string password = properties["POSTGRES_PASSWORD"];
            string database = properties["POSTGRES_DB"];
            string server = properties["POSTGRES_SERVER"];
            string dataStorage = properties["RAW_DATA"];

            IDatabase db = DatabaseFactory.GetDatabase(DatabaseType.Postgres, user, password, database, server);
            
            Program p = new Program(db);

            string filepath = Path.Combine(dataStorage, "dblp_xml_dump", "data");

            p.ReadPublications(filepath);

            Console.WriteLine("Done");
            Console.ReadKey();

        }

        private readonly IDatabase _database;

        private Program(IDatabase database)
        {
            _database = database;
        }

        private void ReadPublications(string filepath)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.DTD,
                XmlResolver = new XmlUrlResolver()
            };

            long id = 0;

            using (XmlReader reader = XmlReader.Create(filepath, settings))
            {
                long x = 0;
                while (reader.Read())
                {
                    x++;
                    if (reader.NodeType == XmlNodeType.Element && reader.Depth == 1)
                    {
                        id++;
                        XElement element = XNode.ReadFrom(reader) as XElement;
                        if (element != null)
                        {
                            List<Dictionary<string, string>> sets = ProcessElement(element, id);
                            ConvertSets(sets);
                        }
                    }
                    if (x % 100000 == 0)
                    {
                        DumpDataSetsToDb();
                    }
                }
                DumpDataSetsToDb();
            }
        }

        private void DumpDataSetsToDb()
        {
            Console.WriteLine("dumping...");
            foreach (KeyValuePair<string, DataTable> kv in tables)
            {
                _database.WriteToDb(schema, kv.Key, kv.Value);
                kv.Value.Clear();
            }
        }
        

        private List<Dictionary<string, string>> ProcessElement(XElement element, long id, long parentObjectId = 0, string parentObjectType = null)
        {
            //Console.WriteLine(element.Name);
            List<Dictionary<string, string>> sets = new List<Dictionary<string, string>>();
            Dictionary<string, string> values = new Dictionary<string, string>();
            values["$_object_type"] = element.Name.ToString();
            values["$_object_id"] = id.ToString();

            if (parentObjectId != 0)
            {
                values["$_parent_object_id"] = parentObjectId.ToString();
                values["$_parent_object_type"] = parentObjectType.ToString();
            }
            foreach (XAttribute attr in element.Attributes())
            {
                values[attr.Name.ToString()] = attr.Value.ToString();
            }
            sets.Add(values);
            if (element.Name.ToString().Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                values[element.Name.ToString()] = element.Value.ToString();
            }
            else if (element.HasElements)
            {
                long subId = 0;
                foreach (XElement subElement in element.Elements())
                {
                    subId++;
                    sets.AddRange(ProcessElement(subElement, subId, id, element.Name.ToString()));
                }
            }
            else
            {
                values[element.Name.ToString()] = element.Value.ToString();
            }
            return sets;
        }

        private Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        private void ConvertSets(List<Dictionary<string, string>> sets)
        {
            foreach (Dictionary<string, string> dict in sets)
            {
                string objectType = dict["$_object_type"];
                if (!tables.ContainsKey(objectType))
                {
                    tables.Add(objectType, new DataTable());
                }
                DataTable dt = tables[objectType];
                foreach (string key in dict.Keys)
                {
                    if (!dt.Columns.Contains(key))
                    {
                        Type type;
                        if (key.Equals("$_object_id") || key.Equals("$_parent_object_id"))
                            type = typeof(int);
                        else
                            type = typeof(string);
                        
                        DataColumn dataColumn = new DataColumn(key, type);
                        dt.Columns.Add(dataColumn);
                    }
                }
                DataRow dr = dt.NewRow();
                foreach (KeyValuePair<string, string> kv in dict)
                {
                    if (kv.Key.Equals("$_object_id") || kv.Key.Equals("$_parent_object_id"))
                        dr[kv.Key] = int.Parse(kv.Value);
                    else
                        dr[kv.Key] = kv.Value;

                }
                dt.Rows.Add(dr);
            }
        }

    }
}
