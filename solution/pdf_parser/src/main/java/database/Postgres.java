package database;

import document.Section;

import java.sql.*;
import java.util.ArrayList;
import java.util.Properties;
import java.util.logging.Logger;

public class Postgres implements Database {

    private Connection conn;

    private final static Logger LOGGER = Logger.getLogger(Postgres.class.getName());

    private final static String SCHEMA_NAME = "lncs_front_matter";

    public Postgres(String username, String password, String database) {
        LOGGER.info("Initializing Postgres connection");
        String url = "jdbc:postgresql://localhost/" + database;
        Properties props = new Properties();
        props.setProperty("user",username);
        props.setProperty("password",password);
        try {
            this.conn = DriverManager.getConnection(url, props);
            createSchema();

        } catch (SQLException e) {
            e.printStackTrace();
        }
        init();
    }

    private void init() {
        createSchema();
        createTableSectionInfo();
    }

    private void createSchema() {
        String query = "CREATE SCHEMA IF NOT EXISTS " + SCHEMA_NAME;
        executeQueryNonResult(query);
    }

    public void createTableSectionInfo() {
        ArrayList<String> columns = new ArrayList<String>() {
            {
                add("id SERIAL PRIMARY KEY");
                add("run_id BIGINT");
                add("filename TEXT");
                add("title TEXT");
                add("content TEXT");
                add("number_of_columns INT");
                add("number_of_text_parts INT");
                add("all_values_contain_commas BOOLEAN");
                add("parser TEXT");
            }
        };
        StringBuilder sb = new StringBuilder();
        sb.append("CREATE TABLE IF NOT EXISTS " + SCHEMA_NAME + ".section"+ " (" + System.lineSeparator() );
        sb.append(String.join(", " + System.lineSeparator(), columns));
        sb.append(");");
        executeQueryNonResult(sb.toString());
    }

    private void executeQueryNonResult(String query){
        LOGGER.info("Executing statement: " + System.lineSeparator() + query);
        Statement st = null;
        try {
            st = conn.createStatement();
            st.execute(query);
            st.close();
        } catch (SQLException e) {
            e.printStackTrace();
        } finally {
            try {
                if (st != null) {
                    st.close();
                }
            } catch (SQLException e) {
                e.printStackTrace();
            }
        }

    }

    @Override
    public void saveSection(long runId, String filename, Section section, String parser) {
        StringBuilder sb = new StringBuilder();
        sb.append("INSERT INTO " + SCHEMA_NAME + ".section (" +
                "run_id, " +
                "filename, title, content, number_of_columns, " +
                "number_of_text_parts, all_values_contain_commas, parser)" + System.lineSeparator());
        sb.append("VALUES (" + System.lineSeparator());
        sb.append(runId + ", ");
        sb.append("'" + filename + "', ");
        sb.append("'" + section.getTitle() + "', ");
        sb.append("'" + section.getContent() + "', ");
        sb.append(section.getSectionInfo().getNumberOfColumns() + ", ");
        sb.append(section.getSectionInfo().getNumberOfTextParts() + ", ");
        sb.append(section.getSectionInfo().isAllValuesContainsCommas() + ", ");
        sb.append("'" + parser + "'");
        sb.append(");" + System.lineSeparator());
        executeQueryNonResult(sb.toString());
    }
}
