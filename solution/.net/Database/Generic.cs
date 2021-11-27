using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Database
{
    abstract class Generic : IDatabase
    {
        protected Dictionary<string, int> DataTableToColumns(DataTable dt)
        {
            Dictionary<string, int> columnsToReturn = new Dictionary<string, int>();
            foreach (DataColumn dc in dt.Columns)
            {
                string maxString = dt.AsEnumerable()
                     .Select(row => row[dc].ToString())
                     .OrderByDescending(st => st.Length).FirstOrDefault();
                // leeg veld
                int stringLength = 1;
                if (maxString != null && maxString.Length > 1)
                {
                    stringLength = maxString.Length;
                }
                columnsToReturn.Add(dc.ColumnName, stringLength);
            }
            return columnsToReturn;
        }

        public abstract ulong WriteToDb(string schemaName, string tableName, DataTable dt);

        public abstract DataTable GetData(string query);
    }
}
