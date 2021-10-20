package parser;

import document.Section;
import document.SectionInfo;
import grid.UnbalancedGrid;
import org.apache.logging.log4j.Level;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.apache.logging.log4j.core.config.Configurator;
import org.junit.Before;

import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class GridParserFactoryTests {

    final Logger logger = LogManager.getLogger(SectionInfoFactory.class.getName());

//    @Before
//    public void setUp() {
//        final Logger logger = LogManager.getLogger(SectionInfoFactory.class.getName());
////        Configurator.setAllLevels("", Level.ALL);
//    }

    private GridParser getParser(SectionToGridConverter sut, Section section) {
        UnbalancedGrid<TextPart> grid = sut.convert(section.getContent());
        System.out.println(grid);
        SectionInfo sectionInfo = SectionInfoFactory.GetSessionInfo(grid);
        System.out.println(sectionInfo);
        GridParser parser = null;
        try {
            parser = GridParserFactory.getInstance().GetParser(sectionInfo, grid);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return parser;
    }

    // conf_dlt_2014
    @org.junit.Test
    public void test1() {
        SectionToGridConverter sut = new SectionToGridConverter();
        Section section = new Section("Program Committee");

        GridParser parser = getParser(sut, section);
        List<Member> result = parser.parse();

        assertEquals("ZeroParser", parser.getName());
        assertEquals(result.size(), 0);
    }

    @org.junit.Test
    public void test2() {
        SectionToGridConverter sut = new SectionToGridConverter();
        Section section = new Section("Program Committee");
        section.addToContent("");
        section.addToContent("           Jorge Almeida                       University of Porto, Portugal");
        section.addToContent("           Marie-Pierre Beal                   Universite Paris-Est, France");
        section.addToContent("           Olivier Carton                      Universite Paris Diderot, France");
        section.addToContent("");
        section.addToContent("");
        section.addToContent("");

        GridParser parser = getParser(sut, section);

        List<Member> result = parser.parse();;

        assertEquals("FirstNameLastNameAffiliationParser", parser.getName());
        assertEquals(3, result.size());
    }

    @org.junit.Test
    public void threeColumnNames() {
        SectionToGridConverter sut = new SectionToGridConverter();
        Section section = new Section("Program Committee");
        section.addToContent("           Gusev, Vladimir               Nicaud, Cyril                Valentin, Oriol");
        section.addToContent("           Haase, Christoph              Niskanen, Reino              Vandomme, Elise");
        section.addToContent("           Harju, Tero                   Palano, Beatrice             Vanier, Pascal");
        section.addToContent("           Hirvensalo, Mika              Perrin, Dominique            Vialette, Stephane");
        section.addToContent("           Holzer, Markus                Plyushchenko, Andrei         Vorel, Vojtech");
        section.addToContent("           Huova, Mari                   Quaas, Karin                 Walen, Tomasz");
        section.addToContent("           Jez, Artur                    Ravikumar, Bala              Wang, Bow-Yaw");
        section.addToContent("           Jungers, Raphael              Reis, Rogerio                Yu, Fang");
        section.addToContent("           Kari, Jarkko                  Richomme, Gwenael");
        section.addToContent("           Klima, Ondrej                 Rodaro, Emanuele");

        GridParser parser = getParser(sut, section);
        List<Member> result = parser.parse();

        assertEquals("AllLastNameFirstNameParser", parser.getName());
        assertEquals(28, result.size());
    }

    @org.junit.Test
    public void twoColumnsNames() {
        SectionToGridConverter sut = new SectionToGridConverter();
        Section section = new Section("Program Committee");
        section.addToContent("           Gusev, Vladimir               Nicaud, Cyril                ");
        section.addToContent("           Haase, Christoph              Niskanen, Reino              ");
        section.addToContent("           Harju, Tero                   Palano, Beatrice            ");
        section.addToContent("           Hirvensalo, Mika              Perrin, Dominique            ");
        section.addToContent("           Holzer, Markus                Plyushchenko, Andrei      ");
        section.addToContent("           Huova, Mari                   Quaas, Karin            ");
        section.addToContent("           Jez, Artur                    Ravikumar, Bala              ");
        section.addToContent("           Jungers, Raphael              Reis, Rogerio             ");
        section.addToContent("           Kari, Jarkko                  Richomme, Gwenael");
        section.addToContent("           Klima, Ondrej                 Rodaro, Emanuele");

        GridParser parser = getParser(sut, section);
        List<Member> result = parser.parse();

        assertEquals("AllLastNameFirstNameParser", parser.getName());
        assertEquals(20, result.size());
    }

    @org.junit.Test
    public void threeLastFirstAffiliation() {
        SectionToGridConverter sut = new SectionToGridConverter();
        Section section = new Section("Program Committee");
        section.addToContent("Abe                 Shigeo           Kobe University");
        section.addToContent("Agell               Nuria            Ramon Llull University");
        section.addToContent("Aiolli              Fabio            Pisa Universit");

        GridParser parser = getParser(sut, section);
        List<Member> result = parser.parse();

        assertEquals("ThreeLastFirstAffParser", parser.getName());
        assertEquals(3, result.size());
    }

    @org.junit.Test
    public void OneColumnOddNameEvenAffiliation() {
        SectionToGridConverter sut = new SectionToGridConverter();
        Section section = new Section("Program Committee");
        section.addToContent("Wl odzis law Duch");
        section.addToContent("Torun, Poland & Singapore, ENNS President");
        section.addToContent("                                         ");
        section.addToContent("Danilo Mandic      ");
        section.addToContent("Imperial College London, UK       ");
        section.addToContent("                            ");

        GridParser parser = getParser(sut, section);
        List<Member> result = parser.parse();

        assertEquals("OneColumnOddNameEvenAffiliation", parser.getName());
        assertEquals(2, result.size());
        for (Member m : result) {
            logger.debug(m);
        }
    }



}
