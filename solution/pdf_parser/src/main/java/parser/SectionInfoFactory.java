package parser;

import document.SectionInfo;
import grid.Position;
import grid.UnbalancedGrid;

import java.util.HashMap;
import java.util.List;

public class SectionInfoFactory {
    public static SectionInfo GetSessionInfo(UnbalancedGrid<TextPart> grid) {
        SectionInfo si = new SectionInfo();
        si.setAllValuesContainsCommas(AllValuesContainsComma(grid));
        si.setNumberOfColumns(grid.columnCount());
        si.setNumberOfTextParts(grid.getNumberOfCells());
        HashMap<Integer, Double> commaRatios = new HashMap<>();
        for (int i = 0; i < si.getNumberOfColumns(); i ++) {
            si.getCommaRatios().put(i, columnCommaRatio(grid, i));
        }
        return si;
    }

    private static double columnCommaRatio(UnbalancedGrid<TextPart> grid, int columnIndex) {
        List<Position<TextPart>> positions = grid.getColumn(columnIndex);
        int numberOfPos = positions.size();
        int numberWithCommas = 0;
        for (Position<TextPart> tp : positions) {
            if(tp != null && tp.getElement().getText().contains(",")) {
                numberWithCommas++;
            }
        }
        if (numberOfPos == 0) {
            return 0;
        }
        double commaRatio = (double)numberWithCommas / (double)numberOfPos;
        return commaRatio;
    }

    private static boolean AllValuesContainsComma(UnbalancedGrid<TextPart> grid) {
        boolean returnValue = true;
        for (Position<TextPart> tp : grid.leftRightTopBottomElements()) {
            if (!tp.getElement().getText().contains(",")) {
                returnValue = false;
                break;
            }
        }
        return returnValue;
    }
}
