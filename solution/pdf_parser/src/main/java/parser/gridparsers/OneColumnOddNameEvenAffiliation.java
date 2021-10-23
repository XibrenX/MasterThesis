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

public class OneColumnOddNameEvenAffiliation implements GridParser {

    private UnbalancedGrid<TextPart> grid;
    private final static Logger LOGGER = LoggerFactory.getLogger(OneColumnOddNameEvenAffiliation.class);

    public OneColumnOddNameEvenAffiliation(UnbalancedGrid<TextPart> grid) {
        LOGGER.debug("Created OneColumnOddNameEvenAffiliation");
        this.grid = grid;
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

    @Override
    public String getName() {
        return "OneColumnOddNameEvenAffiliation";
    }
}
