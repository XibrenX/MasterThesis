package database;

public class DatabaseFactory {

    public static Database getDatabase(String server, String username, String password, String database) {
        return new Postgres(server, username, password, database);
    }

}
