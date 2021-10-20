import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.List;
import java.util.Properties;
import java.util.logging.Logger;

import database.Database;
import database.DatabaseFactory;
import document.*;
import grid.UnbalancedGrid;
import org.apache.pdfbox.io.RandomAccessFile;
import org.apache.pdfbox.pdfparser.PDFParser;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.text.PDFTextStripper;
import parser.*;
import textstripper.PDFLayoutTextStripperFontSize;
import textstripper.TextLine;

public class Program {

    private final static Logger LOGGER = Logger.getLogger(Program.class.getName());

    public static void main(String[] args) throws Exception {

        long runId = System.currentTimeMillis();
        LOGGER.info("Run id: " + runId);

//        String string = null;
//        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_wise_2015-1.pdf";
//        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_dlt_2014.pdf";
//        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_icann_2007-2.pdf";
//        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_miccai_2012-3.pdf";

        Properties properties = new Properties();
        String fileName = "./solution/config";
        try (FileInputStream fis = new FileInputStream(fileName)) {
            properties.load(fis);
        } catch (FileNotFoundException ex) {
            LOGGER.warning("Properties file not found.");
        } catch (IOException ex) {
            LOGGER.warning("Unable to read properties file.");
        }
        Database database = DatabaseFactory.getDatabase(
                properties.getProperty("POSTGRES_SERVER"),
                properties.getProperty("POSTGRES_USER"),
                properties.getProperty("POSTGRES_PASSWORD"),
                properties.getProperty("POSTGRES_DB")
        );

        String pathname = properties.getProperty("RAW_DATA") + "/lncs_front_matter/input/" + "conf_icann_2007-2.pdf";

        try {
            File file = new File(pathname);
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

            Position<Section> organisationPosition = null;
            for (Position<Section> p : documentTree.positions()) {
                if (p.getElement().getTitle().toLowerCase().contains(("organization"))) {
                    organisationPosition = p;
                    break;
                }
            }

            SectionToGridConverter sectionToGridConverter = new SectionToGridConverter();

            // Itereer door alle posities binnen de organisatie
            for (Position<Section> p : documentTree.preorder(organisationPosition)) {

                Section section = p.getElement();
//                processSection(section, sectionToGridConverter);

                System.out.println(section.toString());
                UnbalancedGrid<TextPart> grid = getGrid(sectionToGridConverter, section);
                SectionInfo sectionInfo = SectionInfoFactory.GetSessionInfo(grid);
                section.setSectionInfo(sectionInfo);
                GridParser parser = GridParserFactory.getInstance().GetParser(section.getSectionInfo(), grid);

                // schrijf sectioninfo naar een database
                String parserName = null;
                if (parser != null) {
                    parserName = parser.getName();
                }
                database.saveSection(runId, file.getName(), section, parserName);
            }


        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        };
        System.out.println("Done");
    }

    private static UnbalancedGrid<TextPart> getGrid(SectionToGridConverter sectionToGridConverter, Section section) {
        UnbalancedGrid<TextPart> grid = sectionToGridConverter.convert(section.getContent());
        return grid;
    }

    private static void processSection(GridParser parser, Section section, UnbalancedGrid<TextPart> grid) {
        String role = section.getTitle();
        LOGGER.info("Start processing section: " + role + ".");


        if (parser == null) {
            LOGGER.warning("Unable to find appropriate parser.");
        }
        if (parser != null) {
            try {
                List<Member> result = parser.parse();
                for (Member m : result) {
                    m.setRole(role);
//                    System.out.println(m);
                }
                LOGGER.info("Processing section: " + role + " succeeded (" + result.size() + " results).");
            } catch (Exception ex) {
                LOGGER.warning("Exception occurred in the parsing process.");
            }
        }
    }



} // End class
