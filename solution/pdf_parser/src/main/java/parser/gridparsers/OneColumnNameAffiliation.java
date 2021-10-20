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

public class OneColumnNameAffiliation implements GridParser {

    private UnbalancedGrid<TextPart> grid;
    private final static Logger LOGGER = LogManager.getLogger();

    public OneColumnNameAffiliation(UnbalancedGrid<TextPart> grid) {
        LOGGER.debug("Created OneColumnNameAffiliation");
        this.grid = grid;
    }

    @Override
    public List<Member> parse() {
        List<Member> returnValue = new ArrayList<>();
        for (int i = 0; i < grid.rowCount(); i++) {
            List<Position<TextPart>> row = grid.getRow(i);
            if (row.size() > 0) {
                for (Position<TextPart> pos : row) {
                    String text = pos.getElement().getText();
                    String name = text.substring( 0, text.indexOf(",")).trim();
                    String affiliation = text.substring(text.indexOf(",")+1, text.length()).trim();
                    Member member = new Member(name);
                    member.setAffiliation(affiliation);
                    returnValue.add(member);
                }
            }
        }
        return returnValue;
    }

    @Override
    public String getName() {
        return "OneColumnNameAffiliation";
    }

}
