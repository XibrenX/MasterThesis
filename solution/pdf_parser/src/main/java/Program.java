import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.List;
import java.util.logging.Logger;

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

    public static void main(String[] args) {
        String string = null;
//        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_wise_2015-1.pdf";
//        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_dlt_2014.pdf";
        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_icann_2007-2.pdf";
//        String pathname = "C:\\Users\\EwoudWesterbaan\\Desktop\\pdf_test\\conf_miccai_2012-3.pdf";


        try {
            PDFParser pdfParser = new PDFParser(new RandomAccessFile(new File(pathname), "r"));
            pdfParser.parse();
            PDDocument pdDocument = new PDDocument(pdfParser.getDocument());
            PDFTextStripper pdfTextStripper = new PDFLayoutTextStripperFontSize();
            string = pdfTextStripper.getText(pdDocument);

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

                // schrijf sectioninfo naar een database

            }


        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        };
        System.out.println("Done");
    }

    private static UnbalancedGrid<TextPart> getGrid(SectionToGridConverter sectionToGridConverter, Section section) {
        UnbalancedGrid<TextPart> grid = sectionToGridConverter.convert(section);
        return grid;
    }

    private static void processSection(Section section, UnbalancedGrid<TextPart> grid) {
        String role = section.getTitle().replaceAll("\\s+", " ").trim();
        LOGGER.info("Start processing section: " + role + ".");

        GridParser parser = GridParserFactory.getInstance().GetParser(section.getSectionInfo(), grid);
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
