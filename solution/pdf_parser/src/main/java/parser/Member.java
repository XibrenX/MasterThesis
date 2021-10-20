package parser;

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


    public void setAffiliation(String affiliation) {
        this.affiliation = affiliation;
    }

    public void setRole(String role) { this.role = role; }

    @Override
    public String toString() {
        return "{ Name: " + name + ", Affiliation: " + affiliation + ", Role: " + role + " }";
    }

}
