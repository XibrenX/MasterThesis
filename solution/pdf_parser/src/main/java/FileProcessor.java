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

    public void execute() {
        file = new File(path);
        MDC.put("filename", file.getName());
        try {
            LOGGER.info("Start processing {}", file.getName());
            fileId = database.startProcessingFile(runId, file);
            PDFParser pdfParser = new PDFParser(new RandomAccessFile(file, "r"));
            pdfParser.parse();
            PDDocument pdDocument = new PDDocument(pdfParser.getDocument());
            PDFTextStripper pdfTextStripper = new PDFLayoutTextStripperFontSize();
            String string = pdfTextStripper.getText(pdDocument);

            List<TextLine> textLines = ((PDFLayoutTextStripperFontSize)pdfTextStripper).getAllTextLines();

            // for debugging
//            for (TextLine s1 : textLines) {
//                System.out.println(s1);
//            }

            DocumentFactory df = new DocumentFactory();
            Tree<Section> documentTree = df.getDocumentTreePreOrder(textLines);

            String representation = documentTree.preOrderRepresentation();
            LOGGER.info("Document tree representation " + System.lineSeparator() + representation);

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
                processSection(section);
            } // end loop sections

            database.endProcessingFile(fileId, representation, "SUCCEEDED", "");
        } catch (Exception e) {
            LOGGER.error("Error occurred while processing a file", e);
            e.printStackTrace();
            database.endProcessingFile(fileId, null, "FAILED", e.getMessage());
        }
        LOGGER.info("End processing");
        MDC.remove("filename");
    }

    private void processSection(Section section) {
        LOGGER.info("Start processing section:" + System.lineSeparator() + section.detailedRepresentation());
        try {
            SectionToGridConverter sectionToGridConverter = new SectionToGridConverter();
            UnbalancedGrid<TextPart> grid = sectionToGridConverter.convert(section.getContent());
            LOGGER.info("Grid representation:" + System.lineSeparator() + grid);

            SectionInfo sectionInfo = SectionInfoFactory.GetSessionInfo(grid);
            LOGGER.info("Section info:" + System.lineSeparator() + sectionInfo);

            section.setSectionInfo(sectionInfo);
            GridParser parser = GridParserFactory.getInstance().GetParser(section.getSectionInfo(), grid);

            String parserName = parser.getName();
            LOGGER.info("Parser: {}", parserName);

            int sectionId = database.saveSection(runId, section, parserName, fileId, sectionToGridConverter.getMergedLines());

            List<Member> members = parser.parse();
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.append("Members (" + members.size() + "):" + System.lineSeparator());
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
