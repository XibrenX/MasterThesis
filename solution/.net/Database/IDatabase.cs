using System;
using System.Data;

namespace Database
{
    public interface IDatabase
    {
        ulong WriteToDb(string schemaName, string tableName, DataTable dt);

        DataTable GetData(string query);
    }
}
