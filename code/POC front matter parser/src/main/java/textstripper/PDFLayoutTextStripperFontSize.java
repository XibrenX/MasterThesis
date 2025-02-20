/*
 * Author: Jonathan Link
 * Email: jonathanlink[d o t]email[a t]gmail[d o t]com
 * Date of creation: 13.11.2014
 * Version: 2.2.3
 * Description:
 *
 * Version 2.1 uses PDFBox 2.x. Version 1.0 used PDFBox 1.8.x
 * Acknowledgement to James Sullivan for version 2.0
 *
 * What does it DO:
 * This object converts the content of a PDF file into a String.
 * The layout of the texts is transcribed as near as the one in the PDF given at the input.
 *
 * What does it NOT DO:
 * Vertical texts in the PDF file are not handled for the moment.
 *
 * I would appreciate any feedback you could offer. (see my email address above)
 *
 * LICENSE:
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2014-2019 Jonathan Link
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */


package textstripper;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;

import org.apache.pdfbox.pdmodel.PDPage;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
import org.apache.pdfbox.text.PDFTextStripper;
import org.apache.pdfbox.text.TextPosition;
import org.apache.pdfbox.text.TextPositionComparator;

/**
 * Java doc to be completed
 *
 * @author Jonathan Link
 *
 */
public class PDFLayoutTextStripperFontSize extends PDFTextStripper {

    public static final boolean DEBUG = false;
    public static final int OUTPUT_SPACE_CHARACTER_WIDTH_IN_PT = 4;

    private double currentPageWidth;
    private TextPosition previousTextPosition;
    // Created for every page
    private List<TextLine> textLineList;
    // Contains all textlines of the document
    private List<TextLine> allTextLines;

    /**
     * Constructor
     */
    public PDFLayoutTextStripperFontSize() throws IOException {
        super();
        this.previousTextPosition = null;
        this.textLineList = new ArrayList<TextLine>();
        this.allTextLines = new ArrayList<TextLine>();
    }

    /**
     *
     * @param page page to parse
     */
    @Override
    public void processPage(PDPage page) throws IOException {
        PDRectangle pageRectangle = page.getMediaBox();
        if (pageRectangle!= null) {
            this.setCurrentPageWidth(pageRectangle.getWidth());
            super.processPage(page);
            this.previousTextPosition = null;
            this.textLineList = new ArrayList<TextLine>();
        }
    }

    @Override
    protected void writePage() throws IOException {
        List<List<TextPosition>> charactersByArticle = super.getCharactersByArticle();
        for( int i = 0; i < charactersByArticle.size(); i++) {
            List<TextPosition> textList = charactersByArticle.get(i);
            try {
                this.sortTextPositionList(textList);
            } catch ( java.lang.IllegalArgumentException e) {
                System.err.println(e);
            }
            this.iterateThroughTextList(textList.iterator()) ;
        }
        this.writeToOutputStream(this.getTextLineList());
    }

    private void writeToOutputStream(final List<TextLine> textLineList) throws IOException {
        for (TextLine textLine : textLineList) {
            allTextLines.add(textLine);
            char[] line = textLine.getLine().toCharArray();
            super.getOutput().write(line);
            super.getOutput().write('\n');
            super.getOutput().flush();
        }
    }

    /*
     * In order to get rid of the warning:
     * TextPositionComparator class should implement Comparator<TextPosition> instead of Comparator
     */
    @SuppressWarnings("unchecked")
    private void sortTextPositionList(final List<TextPosition> textList) {
        TextPositionComparator comparator = new TextPositionComparator();
        Collections.sort(textList, comparator);
    }

    private void writeLine(final List<TextPosition> textPositionList) {
        if ( textPositionList.size() > 0 ) {
            TextLine textLine = this.addNewLine();
            boolean firstCharacterOfLineFound = false;
            for (TextPosition textPosition : textPositionList ) {
                Character character = CharacterFactory.createCharacterFromTextPosition(textPosition, this.getPreviousTextPosition(), firstCharacterOfLineFound);
                textLine.writeCharacterAtIndex(character);
                this.setPreviousTextPosition(textPosition);
                firstCharacterOfLineFound = true;
            }
        } else {
            this.addNewLine(); // white line
        }
    }

    private void iterateThroughTextList(Iterator<TextPosition> textIterator) {
        List<TextPosition> textPositionList = new ArrayList<TextPosition>();

        while ( textIterator.hasNext() ) {
            TextPosition textPosition = (TextPosition)textIterator.next();
            int numberOfNewLines = this.getNumberOfNewLinesFromPreviousTextPosition(textPosition);
            if ( numberOfNewLines == 0 ) {
                textPositionList.add(textPosition);
            } else {
                this.writeTextPositionList(textPositionList);
                this.createNewEmptyNewLines(numberOfNewLines);
                textPositionList.add(textPosition);
            }
            this.setPreviousTextPosition(textPosition);
        }
        if (!textPositionList.isEmpty()) {
            this.writeTextPositionList(textPositionList);
        }
    }

    private void writeTextPositionList(final List<TextPosition> textPositionList) {
        this.writeLine(textPositionList);
        textPositionList.clear();
    }

    private void createNewEmptyNewLines(int numberOfNewLines) {
        for (int i = 0; i < numberOfNewLines - 1; ++i) {
            this.addNewLine();
        }
    }

    private int getNumberOfNewLinesFromPreviousTextPosition(final TextPosition textPosition) {
        TextPosition previousTextPosition = this.getPreviousTextPosition();
        if ( previousTextPosition == null ) {
            return 1;
        }

        float textYPosition = Math.round( textPosition.getY() );
        float previousTextYPosition = Math.round( previousTextPosition.getY() );

        if ( textYPosition > previousTextYPosition && (textYPosition - previousTextYPosition > 5.5) ) {
            double height = textPosition.getHeight();
            int numberOfLines = (int) (Math.floor( textYPosition - previousTextYPosition) / height );
            numberOfLines = Math.max(1, numberOfLines - 1); // exclude current new line
            if (DEBUG) System.out.println(height + " " + numberOfLines);
            return numberOfLines ;
        } else {
            return 0;
        }
    }

    private TextLine addNewLine() {
        TextLine textLine = new TextLine(this.getCurrentPageWidth());
        textLineList.add(textLine);
        return textLine;
    }

    private TextPosition getPreviousTextPosition() {
        return this.previousTextPosition;
    }

    private void setPreviousTextPosition(final TextPosition setPreviousTextPosition) {
        this.previousTextPosition = setPreviousTextPosition;
    }

    private int getCurrentPageWidth() {
        return (int) Math.round(this.currentPageWidth);
    }

    private void setCurrentPageWidth(double currentPageWidth) {
        this.currentPageWidth = currentPageWidth;
    }


    private List<TextLine> getTextLineList() {
        return this.textLineList;
    }

    public List<TextLine> getAllTextLines() {
        return this.allTextLines;
    }

}