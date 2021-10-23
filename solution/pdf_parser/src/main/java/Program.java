import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.List;
import java.util.Properties;

import database.Database;
import database.DatabaseFactory;
import document.*;
import grid.UnbalancedGrid;
import org.apache.pdfbox.io.RandomAccessFile;
import org.apache.pdfbox.pdfparser.PDFParser;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.text.PDFTextStripper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import parser.*;
import textstripper.PDFLayoutTextStripperFontSize;
import textstripper.TextLine;

public class Program {

    private final static Logger LOGGER = LoggerFactory.getLogger(Program.class);
    private final static String PROPERTIES_FILE = "./solution/config";
    private final static String RAW_DATA_SUB_DIRECTORY = "/lncs_front_matter/input/";

    private final Properties properties;
    private final Database database;
    private final long runId;

    private Program(Properties properties, Database database) {
        this.properties = properties;
        this.database = database;
        this.runId = System.currentTimeMillis();
        LOGGER.info("Run id: " + runId);
    }

    public void execute() throws Exception {
        LOGGER.info("Start executing");
//  + "conf_icann_2007-2.pdf"
        String directoryPath = properties.getProperty("RAW_DATA") + RAW_DATA_SUB_DIRECTORY;
        File directory = new File(directoryPath);
        String[] filepaths = directory.list();
        int numberOfFiles = filepaths.length;
        LOGGER.info("Number of files to process: {}", numberOfFiles);
        String filepath = directoryPath + "conf_icann_2007-2.pdf";
        FileProcessor fp = new FileProcessor(filepath, database, runId);
        fp.execute();
    }
    // End class

    public static void main(String[] args) throws Exception {
        Properties properties = new Properties();
        try (FileInputStream fis = new FileInputStream(PROPERTIES_FILE)) {
            properties.load(fis);
        } catch (FileNotFoundException ex) {
            LOGGER.warn("Properties file not found.");
        } catch (IOException ex) {
            LOGGER.warn("Unable to read properties file.");
        }
        Database database = DatabaseFactory.getDatabase(
                properties.getProperty("POSTGRES_SERVER"),
                properties.getProperty("POSTGRES_USER"),
                properties.getProperty("POSTGRES_PASSWORD"),
                properties.getProperty("POSTGRES_DB")
        );
        Program p = new Program(properties, database);
        p.execute();
    }
}
