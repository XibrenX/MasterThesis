using System;
using System.Data;

namespace Database
{
    public interface IDatabase
    {
        void WriteToDb(string schemaName, string tableName, DataTable dt);
    }
}
