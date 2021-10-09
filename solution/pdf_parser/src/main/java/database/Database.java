package database;

import document.Section;

public interface Database {
    void saveSection(long runId, String filename, Section section, String parser);
}
