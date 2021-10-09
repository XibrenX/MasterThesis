package parser;

import document.SectionInfo;
import grid.UnbalancedGrid;
import org.jetbrains.annotations.NotNull;
import parser.gridparsers.AllNamesParser;
import parser.gridparsers.FirstNameLastNameAffiliationParser;
import parser.gridparsers.ZeroParser;

public class GridParserFactory {

    private static GridParserFactory _instance = null;

    private static double COMMARATIO = 0.4;

    public static GridParserFactory getInstance() {
        if (_instance == null) {
            _instance = new GridParserFactory();
        }
        return _instance;
    }

    public GridParser GetParser(@NotNull SectionInfo sectionInfo, UnbalancedGrid<TextPart> grid) {

        if (sectionInfo.getNumberOfTextParts() == 0) {
            System.out.println("Return ZeroParser");
            return new ZeroParser(grid);
        }

        if (sectionInfo.getNumberOfColumns() == 2) {
            if (sectionInfo.getCommaRatios().get(0) < COMMARATIO && sectionInfo.getCommaRatios().get(1) > COMMARATIO) {
                return new FirstNameLastNameAffiliationParser(grid);
            }
        }

        if (sectionInfo.getNumberOfColumns() >= 2) {
            return new AllNamesParser(grid);
        }

        System.out.println("Unable to find appropriate parser.");
        return null;
    }


}
