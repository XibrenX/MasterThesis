import database.Database;
import document.*;
import grid.UnbalancedGrid;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.apache.pdfbox.io.RandomAccessFile;
import org.apache.pdfbox.pdfparser.PDFParser;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.text.PDFTextStripper;
import org.slf4j.MDC;
import parser.*;
import textstripper.PDFLayoutTextStripperFontSize;
import textstripper.TextLine;
import java.io.File;
import java.util.List;

/**
 * Responsibility is to process a file.
 */
public class FileProcessor {

    private final static Logger LOGGER = LoggerFactory.getLogger(FileProcessor.class);

    private final String path;
    private final Database database;
    private final long runId;

    private int fileId;
    private File file;

    public FileProcessor(String path, Database database, long runId) {
        this.path = path;
        this.database = database;
        this.runId = runId;
    }

    /**
     * Hier wordt het document daadwerkelijk verwerkt.
     */
    public void execute() {
        file = new File(path);
        MDC.put("filename", file.getName());
        try {
            LOGGER.info("Start processing {}", file.getName());
            fileId = database.startProcessingFile(runId, file);
            PDFParser pdfParser = new PDFParser(new RandomAccessFile(file, "r"));
            pdfParser.parse();
            PDDocument pdDocument = new PDDocument(pdfParser.getDocument());
            try {
                PDFTextStripper pdfTextStripper = new PDFLayoutTextStripperFontSize();
                pdfTextStripper.getText(pdDocument);
                
                // Tekstregels van het document
                List<TextLine> textLines = ((PDFLayoutTextStripperFontSize) pdfTextStripper).getAllTextLines();

                // Maak een tree structure van het document
                DocumentFactory df = new DocumentFactory();
                Tree<Section> documentTree = df.getDocumentTreePreOrder(textLines);
                String representation = documentTree.preOrderRepresentation();
                LOGGER.info("Document tree representation " + System.lineSeparator() + representation);

                // The position to start processing from. We are looking for the organization section.
                Position<Section> organisationPosition = null;
                for (Position<Section> p : documentTree.positions()) {
                    if (p.getElement().getTitle().toLowerCase().contains(("organization"))) {
                        organisationPosition = p;
                        break;
                    }
                }
                if (organisationPosition == null) {
                    throw new Exception("No organisation position found");
                }
                LOGGER.info("Organisation position: {}", organisationPosition.getElement().getTitle());


                // Itereer door alle posities binnen de organisatie
                for (Position<Section> p : documentTree.preorder(organisationPosition)) {
                    Section section = p.getElement();
                    // Verwerk de section
                    processSection(section);
                } // end loop sections

                database.endProcessingFile(fileId, representation, "SUCCEEDED", "");
            } catch (Exception e) {
                throw e;
            } finally {
                pdDocument.close();
            }
        } catch (Exception e) {
            LOGGER.error("Error occurred while processing a file", e);
            e.printStackTrace();
            database.endProcessingFile(fileId, null, "FAILED", e.getMessage());
        }
        LOGGER.info("End processing");
        MDC.remove("filename");
    }

    /**
     * Verwerkt een section.
     * @param section
     */
    private void processSection(Section section) {
        LOGGER.info("Start processing section:" + System.lineSeparator() + section.detailedRepresentation());
        try {
            // Converteer de section naar een grid
            SectionToGridConverter sectionToGridConverter = new SectionToGridConverter();
            UnbalancedGrid<TextPart> grid = sectionToGridConverter.convert(section.getContent());
            LOGGER.info("Grid representation:" + System.lineSeparator() + grid);

            // Verzamel statistieken over de grid
            SectionInfo sectionInfo = SectionInfoFactory.GetSessionInfo(grid);
            LOGGER.info("Section info:" + System.lineSeparator() + sectionInfo);
            section.setSectionInfo(sectionInfo);

            // Haal een parser op, gegeven de grid en de statistieken.
            GridParser parser = GridParserFactory.getInstance().GetParser(section.getSectionInfo(), grid);
            String parserName = parser.getName();
            LOGGER.info("Parser: {}", parserName);

            // Sla de section op in de database
            int sectionId = database.saveSection(runId, section, parserName, fileId, sectionToGridConverter.getMergedLines());

            // Haal de members uit de section
            List<Member> members = parser.parse();
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.append("Members (" + members.size() + "):" + System.lineSeparator());

            // Sla de members op
            for (Member m : members) {
                m.setRole(section.getTitle());
                database.saveMember(runId, sectionId, m);
                logBuilder.append("\t" + m + System.lineSeparator());
            }
            LOGGER.info(logBuilder.toString());
        } catch (Exception ex) {
            LOGGER.error("Error occurred while processing a section", ex);
        }
        LOGGER.info("End processing section {}", section.getTitle());
    }

}
