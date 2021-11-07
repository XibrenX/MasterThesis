namespace Database
{
    public enum DatabaseType
    {
        Postgres
    }

    public class DatabaseFactory
    {
        public static IDatabase GetDatabase(DatabaseType type, string user, string password, string database, string server)
        {
            if (type == DatabaseType.Postgres)
            {
                return new Postgres(user, password, database, server);
            }
            return null;
        }
    }
}
