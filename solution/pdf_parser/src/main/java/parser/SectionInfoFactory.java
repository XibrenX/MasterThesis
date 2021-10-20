package parser;

import document.SectionInfo;
import grid.Position;
import grid.UnbalancedGrid;

import java.util.ArrayList;
import java.util.List;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

public class SectionInfoFactory {

    private static final Logger logger = LogManager.getLogger(SectionInfoFactory.class.getName());

    public static SectionInfo GetSessionInfo(UnbalancedGrid<TextPart> grid) {
        logger.debug("Start GetSessionInfo");
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
        logger.debug("Getting columnRatio for columnIndex {} rows {} searcharguments: {}",
                columnIndex, rows, searchArguments);
        List<Position<TextPart>> positions = grid.getColumn(columnIndex);
        int numberOfRows = positions.size();
        int numberOfProcessedRows = 0;
        int resultCounter = 0;
        for (int rowIndex = 0; rowIndex < numberOfRows; rowIndex++) {
            if (
                    // rowIndex+1 omdat index is zero-based, maar
                    // ODD en EVEN zijn 'domein'-begrippen
                    rows == Rows.ALL
                    || (rows == Rows.ODD && (rowIndex+1) % 2 == 1)
                    || (rows == Rows.EVEN && (rowIndex+1) % 2 == 0)
            ) {
                logger.debug("Processing rowIndex: {} (rowNumber: {})", rowIndex, rowIndex+1 );
                numberOfProcessedRows++;
                if (positions.get(rowIndex) != null) {
                    for (String s : searchArguments) {
                        if (positions.get(rowIndex).getElement().getText().toLowerCase().contains(s)) {
                            resultCounter++;
                            break;
                        }
                    }
                }
            }
        }
        double ratio = 0;
        if (numberOfRows != 0) {
            ratio = (double) resultCounter / (double) numberOfRows;
        }
        logger.debug("Result: numberOfRows: {}, resultCounter: {}, numberOfProcessedRows: {}, ratio: {}",
                numberOfRows,
                resultCounter,
                numberOfProcessedRows,
                ratio);
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
