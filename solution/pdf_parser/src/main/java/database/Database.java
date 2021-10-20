package database;

import document.Section;
import parser.Member;

import java.io.File;

public interface Database {
    int saveSection(long runId, String filename, Section section, String parser, int fileId);

    int startProcessingFile(long runId, File file);

    void endProcessingFile(int fileId, String representation, String status, String message);

    void saveMember(long runId, int sectionId, Member m);
}
