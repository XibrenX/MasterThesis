package parser;

import document.SectionInfo;
import grid.Position;
import grid.UnbalancedGrid;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;




public class SectionInfoFactory {
    public static SectionInfo GetSessionInfo(UnbalancedGrid<TextPart> grid) {
        SectionInfo si = new SectionInfo();
        si.setAllValuesContainsCommas(AllValuesContainsComma(grid));
        si.setNumberOfColumns(grid.columnCount());
        si.setNumberOfTextParts(grid.getNumberOfCells());
        si.setCommaRatios(columnRatio(grid, new String[] {","}));
        si.setAffiliationRatios(columnRatio(grid, new String[] {"universi", "instut", "college"}));
        return si;
    }


    private static List<Ratio> columnRatio(UnbalancedGrid<TextPart> grid, String[] searchArguments) {
        List<Ratio> ratios = new ArrayList<>();
        for (int columnIndex = 0; columnIndex < grid.columnCount(); columnIndex++) {
            for (Rows rows : Rows.values()) {
                Ratio ratio = columnRatio(grid, columnIndex, rows, searchArguments);
                ratios.add(ratio);
            }
        }
        return ratios;
    }


    private static Ratio columnRatio(UnbalancedGrid<TextPart> grid, int columnIndex, Rows rows, String[] searchArguments) {
        List<Position<TextPart>> positions = grid.getColumn(columnIndex);
        int numberOfPos = positions.size();
        int resultCounter = 0;
        for (int position = 0; position < numberOfPos; position++) {
            if (
                    rows == Rows.ALL
                    || (rows == Rows.ODD && position % 2 == 1)
                    || (rows == Rows.EVEN && position % 2 == 0)
            ) {
                if (positions.get(position) != null) {
                    for (String s : searchArguments) {
                        if (positions.get(position).getElement().getText().toLowerCase().contains(s)) {
                            resultCounter++;
                            break;
                        }
                    }
                }
            }
        }
        double ratio = 0;
        if (numberOfPos != 0) {
            ratio = (double) resultCounter / (double) numberOfPos;
        }
        return new Ratio(columnIndex, rows, ratio);
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
