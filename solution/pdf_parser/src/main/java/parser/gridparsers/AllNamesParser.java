package parser.gridparsers;

import grid.Position;
import grid.UnbalancedGrid;
import parser.GridParser;
import parser.Member;
import parser.TextPart;

import java.util.ArrayList;
import java.util.List;

public class AllNamesParser implements GridParser {

    private UnbalancedGrid<TextPart> grid;

    public AllNamesParser(UnbalancedGrid<TextPart> grid) {
        System.out.println("Created AllNamesParser");
        this.grid = grid;
    }

    @Override
    public List<Member> parse() {
        List<Member> returnValue = new ArrayList<>();
        for (int i = 0; i < grid.rowCount(); i++) {
            List<Position<TextPart>> row = grid.getRow(i);
            if (row.size() > 0) {
                for (Position<TextPart> pos : row) {
                    String name = pos.getElement().getText();
                    Member member = new Member(name);
                    returnValue.add(member);
                }
            }
        }
        return returnValue;
    }
}
