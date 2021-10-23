package parser;

import com.google.gson.Gson;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Member {

    private final static Logger LOGGER = LoggerFactory.getLogger(Member.class);

    private final String name;
    private String affiliation;
    private String role;
    private String firstname;

    private String lastname;

    public Member(String name) {

        this.name = name;
        if (name.contains(",")) {
            String[] parts = name.split(",");
            this.firstname = parts[1];
            this.lastname = parts[0];
        }
    }

    public Member(String firstname, String lastname) {
        this.lastname = lastname;
        this.firstname = firstname;
        this.name = lastname + ", " + firstname;
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
