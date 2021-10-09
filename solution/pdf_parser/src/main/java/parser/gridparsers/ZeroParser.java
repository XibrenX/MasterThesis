package parser.gridparsers;

import grid.UnbalancedGrid;
import parser.GridParser;
import parser.Member;
import parser.TextPart;

import java.util.ArrayList;
import java.util.List;

public class ZeroParser implements GridParser {

    public ZeroParser(UnbalancedGrid<TextPart> grid) {}

    @Override
    public List<Member> parse() {
        return new ArrayList<Member>();
    }

    @Override
    public String getName() {
        return "ZeroParser";
    }


}
