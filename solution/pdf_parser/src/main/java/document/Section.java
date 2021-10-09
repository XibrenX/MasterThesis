package document;


import java.util.ArrayList;
import java.util.List;

/**
 * Represents a section in the document.
 */
public class Section {

    /**
     * Title of the section
     */
    private String title;

    public SectionInfo getSectionInfo() {
        return sectionInfo;
    }

    public void setSectionInfo(SectionInfo sectionInfo) {
        this.sectionInfo = sectionInfo;
    }

    private SectionInfo sectionInfo;

    /**
     * Content is represented by an ordered list of strings.
     */
    private List<String> content = new ArrayList<>();

    /**
     * @param title of the section
     */
    public Section(String title) {
        this.title = title;
    }

    /**
     * Adds a string to the content
     * @param s
     */
    public void addToContent(String s) {
        this.content.add(s);
    }

    public Iterable<String> getContent() {
        return content;
    }

    public String getTitle() {
        return title.replaceAll("\\s+", " ").trim();
    }

    public void setTitle(String value) {
        title = value;
    }

    /**
     * @return Textual representation of the section
     */
    @Override
    public String toString() {
        StringBuilder sb = new StringBuilder();
        sb.append("Title: " + getTitle().trim() + System.lineSeparator());
        sb.append("Start content:" + System.lineSeparator());
        sb.append("==============================================================" + System.lineSeparator());
        for (String s : content) {
            sb.append(s + System.lineSeparator());
        }
        sb.append("==============================================================" + System.lineSeparator());
        return sb.toString();
    }
}
