package document;

import java.util.HashMap;
import java.util.Map;


public class SectionInfo {
    public int getNumberOfColumns() {
        return numberOfColumns;
    }

    public void setNumberOfColumns(int numberOfColumns) {
        this.numberOfColumns = numberOfColumns;
    }

    public int getNumberOfTextParts() {
        return numberOfTextParts;
    }

    public void setNumberOfTextParts(int numberOfTextParts) {
        this.numberOfTextParts = numberOfTextParts;
    }

    public HashMap<Integer, Double> getCommaRatios() {
        return commaRatios;
    }

    public void setCommaRatios(HashMap<Integer, Double> commaRatios) {
        this.commaRatios = commaRatios;
    }

    public boolean isAllValuesContainsCommas() {
        return allValuesContainsCommas;
    }

    public void setAllValuesContainsCommas(boolean allValuesContainsCommas) {
        this.allValuesContainsCommas = allValuesContainsCommas;
    }

    private int numberOfColumns;
    private int numberOfTextParts;
    private HashMap<Integer, Double> commaRatios = new HashMap<>();
    private boolean allValuesContainsCommas;

    public SectionInfo() {

    }

    public String toString() {
        StringBuilder sb = new StringBuilder();
        sb.append("allValuesContainsCommas:\t" + allValuesContainsCommas + System.lineSeparator());
        sb.append("numberOfColumns:\t" + numberOfColumns + System.lineSeparator());
        sb.append("numberOfTextParts:\t" + numberOfTextParts + System.lineSeparator());
        if (commaRatios.size() > 0) {
            sb.append("commaRatios:" + System.lineSeparator());
            for (Map.Entry<Integer, Double> entry : commaRatios.entrySet()) {
                sb.append("\t" + entry.getKey() + " -> " + entry.getValue() + System.lineSeparator());
            }
        }
        return sb.toString();
    }
}
