package parser;

import document.SectionInfo;
import grid.UnbalancedGrid;
import parser.gridparsers.*;

public class GridParserFactory {

    private static GridParserFactory _instance = null;

    private static final double COMMARATIO = 0.4;
    private static final double AFFILIATIONRATIO = 0;

    public static GridParserFactory getInstance() {
        if (_instance == null) {
            _instance = new GridParserFactory();
        }
        return _instance;
    }

    public GridParser GetParser(SectionInfo sectionInfo, UnbalancedGrid<TextPart> grid) throws Exception {

        if (sectionInfo.getNumberOfTextParts() == 0) {
            System.out.println("Return ZeroParser");
            return new ZeroParser(grid);
        }

        if (sectionInfo.getNumberOfColumns() == 2) {
            if (sectionInfo.getCommaRatio(0) < COMMARATIO && sectionInfo.getCommaRatio(1) > COMMARATIO) {
                return new FirstNameLastNameAffiliationParser(grid);
            }
        }

        if (sectionInfo.getNumberOfColumns() == 3) {
            if (sectionInfo.getAffiliationRatio(0) <= AFFILIATIONRATIO
                && sectionInfo.getAffiliationRatio(1) <= AFFILIATIONRATIO
                && sectionInfo.getAffiliationRatio(2) > AFFILIATIONRATIO
            ) {
                return new ThreeLastFirstAffParser(grid);
            }
        }

        if (sectionInfo.getNumberOfColumns() >= 2) {
            return new AllLastNameFirstNameParser(grid);
        }

        System.out.println("Unable to find appropriate parser.");
        return null;
    }


}
