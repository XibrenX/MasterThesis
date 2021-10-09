package database;

public class DatabaseFactory {

    public static Database getDatabase(String username, String password, String database) {
        return new Postgres(username, password, database);
    }

}
