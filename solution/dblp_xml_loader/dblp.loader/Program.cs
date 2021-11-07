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

namespace dblp.loader
{
    class Program
    {
        private static string dblpFile = @"C:\dblp\dblp.xml";

        private static string dblpOut = @"C:\dblp\dblp_1000.xml";

        private static string connStr = @"Server=localhost;Database=study;Integrated Security=True;";

        private static string schema = "dblp";


        [Obsolete]
        static void Main(string[] args)
        {
            //XmlReaderSettings settings = new XmlReaderSettings()
            //{
            //    DtdProcessing = DtdProcessing.Parse,
            //    ValidationType = ValidationType.DTD,
            //    XmlResolver = new XmlUrlResolver()
            //};
            //XmlReader reader = XmlReader.Create(dblpFile, settings);

            //DataSet dataSet = new DataSet();

            //dataSet.ReadXml(reader);


            //StreamReader sr = new StreamReader(dblpFile 
            //StreamWriter sw = new StreamWriter(dblpOut);
            //int i = 0;
            //bool reading = false;
            //while (i < 1000)
            //{
            //    string s = sr.ReadLine();
            //    //if (reading || (s.Contains("journals/eccc/Wagner11") && !s.Contains("dblpnote")))
            //    //{
            //    //    reading = true;
            //        sw.WriteLine(s);
            //        i++;
            //    //}
            //}


            ReadPublications(dblpFile);

            Console.WriteLine("Done");
            Console.ReadKey();

        }

        private static void ReadPublications(string inputstring)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                DtdProcessing = DtdProcessing.Parse,
                ValidationType = ValidationType.DTD,
                XmlResolver = new XmlUrlResolver()
            };

            long id = 0;

            using (XmlReader reader = XmlReader.Create(dblpFile, settings))
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
                            //yield return element;
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

        private static void DumpDataSetsToDb()
        {
            Console.WriteLine("dumping...");
            foreach (KeyValuePair<string, DataTable> kv in tables)
            {
                DatabaseHelper.WriteToDb(connStr, schema, kv.Key, kv.Value);
                kv.Value.Clear();
            }
        }
        

        private static List<Dictionary<string, string>> ProcessElement(XElement element, long id, long parentObjectId = 0, string parentObjectType = null)
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

        private static Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        private static void ConvertSets(List<Dictionary<string, string>> sets)
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
