package parser;

import com.google.gson.Gson;

public class Member {

    private final String name;
    private String affiliation;
    private String role;
    private String firstname;

    private String lastname;

    public Member(String name) {
        this.name = name;
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

    public void setRole(String role) { this.role = role; }

    @Override
    public String toString() {
        Gson gson = new Gson();
        String jsonString = gson.toJson(this);
        return jsonString;
    }

}
