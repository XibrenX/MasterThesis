using System;
using System.Collections.Generic;
using System.IO;

namespace Helper
{
    public static class PropertiesReader
    {
        public static Dictionary<string, string> ReadProperties(string path)
        {
            Dictionary<string, string> returnValue = new Dictionary<string, string>();
            using StreamReader sr  = new StreamReader(path);
            string line = string.Empty;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith('#') || string.IsNullOrEmpty(line)) continue;
                string[] lineParts = line.Split('=');
                List<string> valueParts = new List<string>();
                for (int i = 1; i < lineParts.Length; i ++)
                {
                    valueParts.Add(lineParts[i]);
                }
                string value = string.Join('=', valueParts);
                returnValue.Add(lineParts[0], value);
            }
            return returnValue;
        }
    }
}
