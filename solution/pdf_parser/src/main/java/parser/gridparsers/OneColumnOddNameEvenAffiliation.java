package parser.gridparsers;

import grid.Position;
import grid.UnbalancedGrid;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import parser.GridParser;
import parser.Member;
import parser.TextPart;

import java.util.ArrayList;
import java.util.List;

public class OneColumnOddNameEvenAffiliation extends Parser {

    public OneColumnOddNameEvenAffiliation(UnbalancedGrid<TextPart> grid) {
        super(grid);
    }

    @Override
    public List<Member> parse() {
        List<Member> returnValue = new ArrayList<>();
        Member member = null;
        for (int i = 0; i < grid.rowCount(); i++) {
            List<Position<TextPart>> row = grid.getRow(i);

            if (i%2 == 0) {
                String name = row.get(0).getElement().getText();
                member = new Member(name);
            } else {
                String affiliation = row.get(0).getElement().getText();
                member.setAffiliation(affiliation);
                returnValue.add(member);
            }
        }
        return returnValue;
    }

}
