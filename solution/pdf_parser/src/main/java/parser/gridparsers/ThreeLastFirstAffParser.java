package parser.gridparsers;

import grid.Position;
import grid.UnbalancedGrid;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import parser.GridParser;
import parser.Member;
import parser.TextPart;

import java.util.ArrayList;
import java.util.List;

// Class to interpreted grids with in the first column the last name,
// second column the firstname and third column the affiliation;
public class ThreeLastFirstAffParser implements GridParser {

    private final static Logger LOGGER = LogManager.getLogger();
    private UnbalancedGrid<TextPart> grid;

    public ThreeLastFirstAffParser(UnbalancedGrid<TextPart> grid) {
        LOGGER.debug("Created ThreeLastFirstAffParser");
        this.grid = grid;
    }

    @Override
    public List<Member> parse() {
        List<Member> returnValue = new ArrayList<>();
        for (int i = 0; i < grid.rowCount(); i++) {
            List<Position<TextPart>> row = grid.getRow(i);
            if (row.size() > 0) {
                String lastname = row.get(0).getElement().getText();
                String firstname = row.get(1).getElement().getText();
                String affiliation = row.get(2).getElement().getText();
                Member member = new Member(lastname, firstname);
                member.setAffiliation(affiliation);
                returnValue.add(member);
            }
        }
        return returnValue;
    }

    @Override
    public String getName() {
        return "ThreeLastFirstAffParser";
    }
}
