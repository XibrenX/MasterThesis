package parser;

import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class SectionToGridConverterTests {

    @org.junit.Test
    public void getColumnRanges_1() {
        SectionToGridConverter sut = new SectionToGridConverter();

        List<Integer> input =  new ArrayList<>();
        input.add(2);
        input.add(1);
        input.add(3);
        input.add(6);
        input.add(5);
        input.add(7);

        List<ColumnRange> intermediateResult = sut.getColumnRanges(input);
        assertEquals(2, intermediateResult.size());

        List<ColumnRange> result = sut.cleanColumnRanges(intermediateResult);
        assertEquals(2, result.size());

    }

    @org.junit.Test
    public void getColumnRanges_2() {
        SectionToGridConverter sut = new SectionToGridConverter();

        List<Integer> input =  new ArrayList<>();
        input.add(1);
        input.add(3);
        input.add(2);
        input.add(5);
        input.add(7);
        input.add(6);

        List<ColumnRange> intermediateResult = sut.getColumnRanges(input);
        assertEquals(4, intermediateResult.size());

        List<ColumnRange> result = sut.cleanColumnRanges(intermediateResult);
        assertEquals(2, result.size());
    }

    @org.junit.Test
    public void getColumnRanges_3() {
        SectionToGridConverter sut = new SectionToGridConverter();

        List<Integer> input =  new ArrayList<>();
        input.add(8);
        input.add(1);
        input.add(6);
        input.add(3);
        input.add(7);
        input.add(2);

        List<ColumnRange> intermediateResult = sut.getColumnRanges(input);
        assertEquals(4, intermediateResult.size());

        List<ColumnRange> result = sut.cleanColumnRanges(intermediateResult);
        assertEquals(2, result.size());
    }

    @org.junit.Test
    public void cleanColumnRanges_3() {
        SectionToGridConverter sut = new SectionToGridConverter();

        List<Integer> input =  new ArrayList<>();
        input.add(0);
        input.add(11);
        input.add(47);

        List<ColumnRange> intermediateResult = sut.getColumnRanges(input);
        assertEquals(3, intermediateResult.size());

        List<ColumnRange> result = sut.cleanColumnRanges(intermediateResult);
        assertEquals(3, result.size());
    }

}
