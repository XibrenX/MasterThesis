package parser;

import document.Section;
import grid.UnbalancedGrid;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Converts a section to members
 */
public class SectionToGridConverter {

    /**
     * Converts a section to a grid
     */
    public UnbalancedGrid<TextPart> convert(Section section) {
        UnbalancedGrid<TextPart> grid = new UnbalancedGrid<>();

        // First step is to determine the columns
        List<Integer> horizontalPositions = new ArrayList<>();

        for(String line : section.getContent()) {
            String[] parts = line.split("\\s\\s+");
            for (String part : parts) {
                if(part.equals("")) continue;
                int position = line.indexOf(part);
                horizontalPositions.add(position);
            }
        }
//        for(Integer hp : horizontalPositions) {
//            System.out.println(hp);
//        }

        List<ColumnRange> rawColumnRanges = getColumnRanges(horizontalPositions);

//        System.out.println("Number of rawColumnRanges: " + rawColumnRanges.size());
//        for (ColumnRange cr : rawColumnRanges) {
//            System.out.println(cr);
//        }

        List<ColumnRange> cleanColumnRanges = cleanColumnRanges(rawColumnRanges);

//        System.out.println("Number of cleanColumnRanges: " + cleanColumnRanges.size());
//        for (ColumnRange cr : cleanColumnRanges) {
//            System.out.println(cr);
//        }


        // Second step is to fill the grid per column
        Collections.sort(cleanColumnRanges);
        for (int columnRangeNum = 0; columnRangeNum < cleanColumnRanges.size(); columnRangeNum++) {
            ColumnRange cr = cleanColumnRanges.get(columnRangeNum);

            int lineNumber = 0;
            for(String line : section.getContent()) {
                String[] parts = line.split("\\s\\s+");
                for (String part : parts) {
                    if(part.equals("")) continue;
                    int position = line.indexOf(part);
                    if (position >= cr.getMinimum() && position <= cr.getMaximum()) {
                        int offset = position - cr.getModus();
                        TextPart tp = new TextPart(part, offset);
                        grid.createCell(tp, lineNumber, columnRangeNum);
                    }
                }
                lineNumber++;
            }
        }

//        System.out.println(grid.toString());


//            for (String part : ) {
//                if (!part.equals("")) {
//                    parts.add(part);
//                }
//            }
//            members.add(createMember(parts.get(0).trim(), parts.get(1).trim()));
//        }
        return grid;
    }

    /**
     *
     * @param positions die voorkomen in de tekst
     */
    public List<ColumnRange> getColumnRanges(List<Integer> positions) {
        List<ColumnRange> ranges = new ArrayList<>();
        for (Integer value : positions) {
//            System.out.println("Processing: " + value);
            boolean valuePlaced = false;

            // Eerst kijken of die binnen een range valt
            for (ColumnRange range : ranges) {
                if (value <= range.getMaximum() && value >= range.getMinimum()) {
                    range.add(value);
                    valuePlaced = true;
                    break;
                }
            }

            // Dan kijken of die aansluitend aan een range valt
            if (!valuePlaced) {
                for (ColumnRange range : ranges) {
                    if (value <= range.getMaximum() + 1 && value >= range.getMinimum() - 1) {
                        range.add(value);
                        valuePlaced = true;
                        break;
                    }
                }
            }

            if (!valuePlaced) {
                // Nieuwe maken
                ColumnRange columnRange = new ColumnRange();
                columnRange.add(value);
                ranges.add(columnRange);
            }
        }

        return ranges;
    }

    /**
     * Public for test purposes.
     */
    public List<ColumnRange> cleanColumnRanges(List<ColumnRange> ranges) {
        List<ColumnRange> returnValues = new ArrayList<>();
        Collections.sort(ranges);
        int i = 0;
        while (i < ranges.size()) {
            ColumnRange current = ranges.get(i);
            int o = i+1;
            while (o < ranges.size()) {
                ColumnRange other = ranges.get(o);
                if (current.getMaximum() + 1 == other.getMinimum()) {
                    // aansluitend
                    // mergen tot 1
                    current.merge(other);
                    o++;
                } else {
                    break;
                }
            }
            returnValues.add(current);
            i = o;
        }
        return returnValues;
    }

}
