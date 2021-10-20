package parser.gridparsers;

import grid.UnbalancedGrid;
import parser.GridParser;
import parser.Member;
import parser.TextPart;

import java.util.List;

public class OneColumnOddNameEvenAffiliation implements GridParser {

    private UnbalancedGrid<TextPart> grid;

    public OneColumnOddNameEvenAffiliation(UnbalancedGrid<TextPart> grid) {
        System.out.println("Created OneColumnOddNameEvenAffiliation");
        this.grid = grid;
    }

    @Override
    public List<Member> parse() {
        return null;
    }

    @Override
    public String getName() {
        return "OneColumnOddNameEvenAffiliation";
    }
}
