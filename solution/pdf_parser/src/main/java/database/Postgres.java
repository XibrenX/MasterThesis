package database;

import document.Section;

import java.io.File;
import java.sql.*;
import java.util.ArrayList;
import java.util.Properties;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import parser.Member;

public class Postgres implements Database {

    private Connection conn;

    private final static Logger LOGGER = LoggerFactory.getLogger(Postgres.class);

    private final static String SCHEMA_NAME = "lncs_front_matter";

    public Postgres(String server, String username, String password, String database) {
        LOGGER.trace("Initializing Postgres connection");
        String url = "jdbc:postgresql://" + server + "/" + database;
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
        createTableFile();
        createTableSection();
        createTableMember();
    }

    private void createSchema() {
        String query = "CREATE SCHEMA IF NOT EXISTS " + SCHEMA_NAME;
        executeQueryNonResult(query);
    }

    public void createTableFile() {
        ArrayList<String> columns = new ArrayList<String>() {
            {
                add("id SERIAL PRIMARY KEY");
                add("run_id BIGINT");
                add("filename TEXT");
                add("status TEXT");
                add("message TEXT");
            }
        };
        createTable("file", columns);
    }

    public void createTableSection() {
        ArrayList<String> columns = new ArrayList<String>() {
            {
                add("id SERIAL PRIMARY KEY");
                add("run_id BIGINT");
                add("file_id BIGINT");
                add("title TEXT");
                add("num_parts INT");
                add("num_section_lines INT");
                add("num_section_lines_non_empty INT");
                add("num_merged_lines INT");
                add("num_grid_rows INT");
                add("num_grid_columns INT");
                add("all_values_contain_commas BOOLEAN");
                add("comma_ratios TEXT");
                add("affiliation_ratios TEXT");
                add("parser TEXT");
            }
        };
        createTable("section", columns);
    }

    public void createTableMember() {
        ArrayList<String> columns = new ArrayList<String>() {
            {
                add("id SERIAL PRIMARY KEY");
                add("run_id BIGINT");
                add("section_id BIGINT");
                add("role TEXT");
                add("name TEXT");
                add("firstname TEXT");
                add("lastname TEXT");
                add("affiliation TEXT");
            }
        };
        createTable("member", columns);
    }

    private void createTable(String name, ArrayList<String> columns) {
        StringBuilder sb = new StringBuilder();
        sb.append("CREATE TABLE IF NOT EXISTS " + SCHEMA_NAME + "." + name + " (" + System.lineSeparator() );
        sb.append(String.join(", " + System.lineSeparator(), columns));
        sb.append(");");
        executeQueryNonResult(sb.toString());
    }

    private void executeQueryNonResult(String query){
        LOGGER.trace("Executing statement: " + System.lineSeparator() + query);
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

    /*
     * WARNING: Sensitive for SQL Injection.
     */
    @Override
    public int saveSection(long runId, Section section, String parser, int fileId, int mergedLines) {
        StringBuilder sb = new StringBuilder();
        sb.append("INSERT INTO " + SCHEMA_NAME + ".section (" +
                "run_id, " +
                "file_id, " +
                "title, " +
                "num_parts, " +
                "num_section_lines, " +
                "num_section_lines_non_empty, " +
                "num_merged_lines, " +
                "num_grid_rows, " +
                "num_grid_columns, " +
                "all_values_contain_commas, " +
                "comma_ratios, " +
                "affiliation_ratios, " +
                "parser" +
                ") " + System.lineSeparator());
        sb.append("VALUES (" + System.lineSeparator());
        sb.append(runId + ", ");
        sb.append(fileId + ", ");
        sb.append("'" + section.getTitle() + "', ");
        sb.append(section.getSectionInfo().getNumberOfTextParts() + ", ");
        sb.append(section.getContentSize() + ", ");
        sb.append(section.getNonEmptyContentSize() + ", ");
        sb.append(mergedLines + ", ");
        sb.append(section.getSectionInfo().getNumberOfRows() + ", ");
        sb.append(section.getSectionInfo().getNumberOfColumns() + ", ");
        sb.append(section.getSectionInfo().isAllValuesContainsCommas() + ", ");
        sb.append("'" + section.getSectionInfo().stringRepCommaRatios() + "', ");
        sb.append("'" + section.getSectionInfo().stringRepAffiliationRatios() + "', ");
        sb.append("'" + parser + "'");
        sb.append(") RETURNING id;" + System.lineSeparator());
        int result = executeQueryIntResult(sb.toString());
        return result;
    }

    private int executeQueryIntResult(String query) {
        LOGGER.trace("Executing statement: " + System.lineSeparator() + query);
        Statement st = null;
        int result = -1;
        try {
            st = conn.createStatement();
            ResultSet rs = st.executeQuery(query);
            while (rs.next())
            {
                result = rs.getInt(1);
            }
            rs.close();
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
        return result;
    }

    /*
     * WARNING: Sensitive for SQL Injection.
     */
    @Override
    public int startProcessingFile(long runId, File file) {
        String query = "INSERT INTO " + SCHEMA_NAME + ".file (run_id, filename) VALUES (" + runId + ", '" + file.getName() + "') RETURNING id";
        int result = executeQueryIntResult(query);
        return result;
    }

    /*
     * WARNING: Sensitive for SQL Injection.
     */
    @Override
    public void endProcessingFile(int fileId, String representation, String status, String message) {
        StringBuilder sb = new StringBuilder();
        sb.append("UPDATE " + SCHEMA_NAME + ".file SET" + System.lineSeparator() +
                "  status = '" + status + "'" + System.lineSeparator() +
                ", message = '" + message + "'" + System.lineSeparator());
        sb.append("WHERE id = " + fileId);
        executeQueryNonResult(sb.toString());
    }

    /*
     * WARNING: Sensitive for SQL Injection.
     */
    @Override
    public void saveMember(long runId, int sectionId, Member m) {
        String query = "INSERT INTO " + SCHEMA_NAME + ".member (run_id, section_id, role, name, firstname, lastname, affiliation) VALUES (" + System.lineSeparator()
                + runId
                + " , " + sectionId
                + " ,'" + m.getRole() + "'"
                + " ,'" + m.getName() + "'"
                + " ,'" + m.getFirstname() + "'"
                + " ,'" + m.getLastname() + "'"
                + " ,'" + m.getAffiliation() + "'"
                + ")";
        executeQueryNonResult(query);
    }
}
