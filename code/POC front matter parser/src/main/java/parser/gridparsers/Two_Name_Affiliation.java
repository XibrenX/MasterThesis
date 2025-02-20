package parser.gridparsers;

import grid.Position;
import grid.UnbalancedGrid;
import parser.Member;
import parser.TextPart;

import java.util.ArrayList;
import java.util.List;

public class Two_Name_Affiliation extends Parser {

    public Two_Name_Affiliation(UnbalancedGrid<TextPart> grid) {
        super(grid);
    }

    @Override
    public List<Member> parse() {
        List<Member> returnValue = new ArrayList<>();
        for (int i = 0; i < grid.rowCount(); i++) {
            List<Position<TextPart>> row = grid.getRow(i);
            if (row.size() > 0) {
                String name = row.get(0).getElement().getText();
                String affiliation = row.get(1).getElement().getText();
                Member member = new Member(name);
                member.setAffiliation(affiliation);
                returnValue.add(member);
            }
        }
        return returnValue;
    }

}
