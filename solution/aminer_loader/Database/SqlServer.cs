using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Database
{
    class SqlServer
    {
        public static void WriteToDb(string connStr, string schemaName, string tableName, DataTable dt)
        {

            if (!SchemaExists(connStr, schemaName))
            {
                CreateSchema(connStr, schemaName);
            }
            Dictionary<string, int> sourceColumns = DataTableToColumns(dt);

            if (!TableExists(connStr, schemaName, tableName))
            {
                string createStmt = GetCreateTableStatement(dt, schemaName, tableName);
                ExecuteStatement(connStr, createStmt);
            }
            else
            {
                Console.WriteLine("Checking if database needs to be adjusted.");
                Dictionary<string, int> columnsToAdd = new Dictionary<string, int>();
                Dictionary<string, int> columnsToChange = new Dictionary<string, int>();
                // check if we need to change
                Dictionary<string, int> columnsFromStaging = GetColumnInfo(connStr, schemaName, tableName);
                foreach (KeyValuePair<string, int> dtColumn in sourceColumns)
                {
                    if (!columnsFromStaging.ContainsKey(dtColumn.Key))
                    {
                        columnsToAdd.Add(dtColumn.Key, dtColumn.Value);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"Comparing: {dtColumn.Key} - DT: {dtColumn.Value} STG: {columnsFromStaging[dtColumn.Key]}");

                        if (dtColumn.Value > columnsFromStaging[dtColumn.Key])
                        {
                            columnsToChange.Add(dtColumn.Key, dtColumn.Value);
                        }
                    }
                }
                // Voeg kolommen toe als die nodig zijn.
                if (columnsToAdd.Count > 0)
                {
                    StringBuilder qry = new StringBuilder();
                    qry.AppendLine($"ALTER TABLE [{schemaName}].[{tableName}] ADD ");
                    List<string> columnAdd = new List<string>();
                    foreach (KeyValuePair<string, int> c in columnsToAdd)
                    {
                        columnAdd.Add($"[{c.Key}] NVARCHAR({c.Value}) NULL");
                    }
                    qry.AppendLine(string.Join(", " + Environment.NewLine, columnAdd));
                    qry.AppendLine(";");
                    ExecuteStatement(connStr, qry.ToString());
                }
                // Maak kolommen groter
                if (columnsToChange.Count > 0)
                {
                    foreach (KeyValuePair<string, int> c in columnsToChange)
                    {
                        string qry = $"ALTER TABLE [{schemaName}].[{tableName}] ALTER COLUMN [{c.Key}] NVARCHAR({c.Value}) NULL";
                        ExecuteStatement(connStr, qry);
                    }
                }
            }



            using (SqlBulkCopy bc = new SqlBulkCopy(connStr))
            {
                bc.DestinationTableName = schemaName + "." + tableName;

                foreach (DataColumn dc in dt.Columns)
                {
                    bc.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                }

                //try
                //{
                bc.WriteToServer(dt);
                //}
                //catch (Exception e)
                //{
                //    //Console.WriteLine(e.Message);
                //}
            }
        }

        public static bool SchemaExists(string connStr, string schemaName)
        {
            string query = $"SELECT * FROM sys.schemas WHERE name = '{schemaName}'";
            using SqlConnection connection = new SqlConnection(connStr);
            connection.Open();
            using SqlCommand sqlCommand = new SqlCommand(query, connection);
            {
                return sqlCommand.ExecuteScalar() != null;
            }
        }

        public static void CreateSchema(string connStr, string schemaName)
        {
            string query = $"CREATE SCHEMA [{schemaName}]";
            ExecuteStatement(connStr, query);
        }

        public static void ExecuteStatement(string connStr, string query)
        {
            using SqlConnection connection = new SqlConnection(connStr);
            connection.Open();
            using SqlCommand sqlCommand = new SqlCommand(query, connection);
            sqlCommand.ExecuteNonQuery();
        }

        private static Dictionary<string, int> DataTableToColumns(DataTable dt)
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

        private static bool TableExists(string connStr, string schema, string tableName)
        {
            string query = $"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{tableName}';";
            using SqlConnection connection = new SqlConnection(connStr);
            connection.Open();
            using SqlCommand sqlCommand = new SqlCommand(query, connection);
            object res = sqlCommand.ExecuteScalar();
            return res != null;
        }

        private static string GetCreateTableStatement(DataTable dt, string schemaName, string TableName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{schemaName}].[{TableName}] (");
            List<string> columnParts = new List<string>
            {
                "[$_valid_on] DATETIME2(0) DEFAULT '2021-04-06 23:00:00'"
            };
            foreach (DataColumn dc in dt.Columns)
            {
                string columnPart = string.Empty;
                string maxString = dt.AsEnumerable()
                    .Select(row => row[dc].ToString())
                    .OrderByDescending(st => st.Length).FirstOrDefault();
                int stringLength = maxString.Length;
                if (stringLength == 0)
                {
                    stringLength = 1;
                }
                columnPart = $"[{dc.ColumnName}] NVARCHAR({stringLength}) NULL";
                columnParts.Add(columnPart);
            }

            string s = string.Join(", " + Environment.NewLine, columnParts);
            sb.AppendLine(s);
            sb.AppendLine(");");
            return sb.ToString();
        }

        public static Dictionary<string, int> GetColumnInfo(string connStr, string schemaName, string tableName)
        {
            string query = $"SELECT [COLUMN_NAME], [CHARACTER_MAXIMUM_LENGTH] FROM INFORMATION_SCHEMA.COLUMNS WHERE [TABLE_SCHEMA] = '{schemaName}' AND[TABLE_NAME] = '{tableName}'; ";
            Dictionary<string, int> columns = new Dictionary<string, int>();
            using SqlConnection connection = new SqlConnection(connStr);
            connection.Open();
            using SqlCommand sqlCommand = new SqlCommand(query, connection);
            using SqlDataReader dr = sqlCommand.ExecuteReader();
            while (dr.Read())
            {
                int length = 1;
                if (!dr.IsDBNull(1))
                {
                    length = dr.GetInt32(1);
                }
                columns.Add(dr.GetString(0), length);
            }
            return columns;
        }
    }
}
