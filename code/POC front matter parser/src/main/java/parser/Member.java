package parser;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

import com.google.gson.Gson;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Member {

    private final static Logger LOGGER = LoggerFactory.getLogger(Member.class);

    private final static Pattern affiliationPattern = Pattern.compile("\\([^\\)]*\\)$");

    private final String name;
    private String affiliation;
    private String role;
    private String firstname;

    private String lastname;

    public Member(String name) {
        name = name.trim().replaceAll("\\s\\s+", " ");
        Matcher matches = affiliationPattern.matcher(name);
        if(matches.find()) {
            this.affiliation = name.substring(matches.start() + 1, matches.end() - 1).trim();
            name = (name.substring(0, matches.start()) + name.substring(matches.end())).trim();
        }

        if (name.contains(",")) {
            String[] parts = name.split(",");
            this.firstname = parts[1].trim();
            this.lastname = parts[0].trim();
            
            if(parts.length > 2) {
                if(this.affiliation == null) {
                    this.name = firstname + ' ' + lastname;
                    this.affiliation = name.substring(parts[0].length() + 1 + parts[1].length() + 1).trim();
                } else{
                    this.name = name;
                }
            } else {
                this.name = firstname + ' ' + lastname;
            }
        } else {
            this.name = name;
            String[] parts = name.split(" ");
            if(parts.length >= 2) {
                this.firstname = parts[0];
                this.lastname = parts[parts.length - 1];
            }
        }
    }

    public Member(String firstname, String lastname) {
        this.lastname = lastname;
        this.firstname = firstname;
        this.name = firstname + " " + lastname;
    }

    public String getName() {
        return this.name;
    }

    public String getFirstname() {
        return this.firstname;
    }


    public String getAffiliation() {
        return affiliation;
    }

    public String getRole() {
        return role;
    }

    public String getLastname() {
        return lastname;
    }

    public void setAffiliation(String affiliation) {
        this.affiliation = affiliation;
    }

    public void setRole(String role) {
        if (this.role == null) {
            this.role = role;
        } else {
            LOGGER.trace("Preventing existing role {} to be overridden with value: {}", this.role, role);
        }
    }

    @Override
    public String toString() {
        Gson gson = new Gson();
        String jsonString = gson.toJson(this);
        return jsonString;
    }

}
