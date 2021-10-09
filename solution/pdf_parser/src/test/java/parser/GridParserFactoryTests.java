package parser;

import document.Section;
import document.SectionInfo;
import grid.UnbalancedGrid;

import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class GridParserFactoryTests {

    private GridParser getParser(SectionToGridConverter sut, Section section) {
        UnbalancedGrid<TextPart> grid = sut.convert(section.getContent());
        SectionInfo sectionInfo = SectionInfoFactory.GetSessionInfo(grid);
        GridParser parser = GridParserFactory.getInstance().GetParser(sectionInfo, grid);
        return parser;
    }

    // conf_dlt_2014
    @org.junit.Test
    public void test1() {
        SectionToGridConverter sut = new SectionToGridConverter();
        Section section = new Section("Program Committee");

        GridParser parser = getParser(sut, section);
        List<Member> result = parser.parse();

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

        assertEquals(20, result.size());
    }

}
