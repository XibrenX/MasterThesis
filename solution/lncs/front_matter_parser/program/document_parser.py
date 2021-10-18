from abc import abstractclassmethod
from typing import List, Tuple
from bs4 import BeautifulSoup, Tag
import codecs
import cssutils
from decimal import Decimal, InvalidOperation
import logging
import itertools
import operator
from functools import total_ordering
import textlib

class ParseError(Exception):
    pass


class CssInterpreter:

    def __init__(self):
        self.selectors = {}


    def build(self, stylesheets):
        selectors = {}
        for stylesheet in stylesheets:
            sheet = cssutils.parseString(stylesheet)
            for rule in sheet:
                if rule.type == rule.STYLE_RULE:
                    style = rule.selectorText
                    selectors[style] = {}
                    for item in rule.style:
                        propertyname = item.name
                        value = item.value
                        selectors[style][propertyname] = value
        self.selectors = selectors


    def get(self, cls) -> dict:
        search_arg = cls
        return_value = {}
        if not cls.startswith('.'):
            search_arg = '.' + cls
        for key, value in self.selectors[search_arg].items():
            # if value.endswith('px'):
            try:
                new_value = Decimal(value.rstrip("px"))
            except InvalidOperation:
                new_value = value
            return_value[key] = new_value
        return return_value


SPLIT_VALUE = "%__SPLIT__%"
UNDO_PREVIOUS_SPACE = "%__UNDO_PREVIOUS_SPACE__%"
DOC_WIDTH = 1024
AFFILIATION_COMMA_TRESHOLD = 0.4

@total_ordering
class Div:
    """
    Div element.
    """
    def __init__(self, element):
        if element is not None:
            self.element = element
            self.text = element.text
        self.attributes = []


    def setattr(self, key, value):
        setattr(self, key, value)
        self.attributes.append(key)


    def parse(self, css_interpreter: CssInterpreter) -> list:
        detail_log = False
        if detail_log:
            logging.debug(f"Parsing: {self}")
        
        column_split_width_treshold = 20
        letter_spacing_treshold = Decimal(9)
        return_value = []

        def process_tag(content, current_left, word_spacing, letter_spacing, width, indent = 0) -> tuple:
            new_indent = indent + 1
            log_prefix = indent * "- "
            if detail_log:
                logging.debug(f"{log_prefix}IN: '{content}'")
                logging.debug(f"{log_prefix}SETTING: word_spacing: {word_spacing} | letter_spacing: {letter_spacing}| width: {width}")

            ret_value = []
            left = current_left
            if isinstance(content, Tag):
                process_subcontent = True
                if content.attrs is not None:
                    class_values = {}
                    for c in content.attrs["class"]:
                        for k, v in css_interpreter.get(c).items():
                            class_values[k] = v

                    if "margin-left" in class_values:
                        margin_left = class_values["margin-left"]
                        if letter_spacing > letter_spacing_treshold:
                            if (letter_spacing + margin_left) <= letter_spacing_treshold:
                                if detail_log:
                                    logging.debug(f"{log_prefix}(letter_spacing + margin_left) LT or EQ letter_spacing_treshold: -> appending UNDO_PREVIOUS_SPACE")
                                ret_value.append(UNDO_PREVIOUS_SPACE)
                    if "width" in class_values:
                        width = class_values["width"]
                    if "letter-spacing" in class_values:
                        letter_spacing = class_values["letter-spacing"]
                    if "word-spacing" in class_values:
                        word_spacing = class_values["word-spacing"]
                    if "left" in class_values:
                        new_left = class_values["left"]
                        if new_left < current_left:
                            left = new_left

                    # if width > letter_spacing_treshold:
                    #     ret_value.append(' ')
                    if "display" in class_values and class_values["display"] == 'inline-block':
                        if detail_log:
                            logging.debug(f"{log_prefix}inline-block, checking width {width}")
                        if (width > column_split_width_treshold
                            or (width == -1 and word_spacing > column_split_width_treshold)):
                            ret_value.append(SPLIT_VALUE)
                        elif (width > letter_spacing_treshold
                            or (width == -1 and word_spacing > letter_spacing_treshold)):
                            ret_value.append(' ')
                            # but if there is also a margin-left, we need to undo this
                            if "margin-left" in class_values:
                                margin_left = class_values["margin-left"]
                                calc_width = width if width > -1 else 0
                                if (calc_width + margin_left) <= letter_spacing_treshold:
                                    if detail_log:
                                        logging.debug(f"{log_prefix}(calc_width + margin_left) LT or EQ letter_spacing_treshold: -> appending UNDO_PREVIOUS_SPACE")
                                    ret_value.append(UNDO_PREVIOUS_SPACE)
                                

                        process_subcontent = False
                            
                if process_subcontent:
                    for c in content.contents:
                        (out_content, out_left) = process_tag(c, left, word_spacing, letter_spacing, width, new_indent)
                        ret_value.extend(out_content)
            else:
                split_text = ''
                if letter_spacing > column_split_width_treshold:
                    if detail_log:
                        logging.debug(f"{log_prefix}letter_spacing GT column_split_width_treshold -> split_text = SPLIT")
                    split_text = SPLIT_VALUE
                elif letter_spacing > letter_spacing_treshold:
                    if detail_log:
                        logging.debug(f"{log_prefix}letter_spacing GT letter_spacing_treshold -> split_text = ' '")
                    split_text = ' '
                v = split_text.join(content)
                if detail_log:
                    logging.debug(f"{log_prefix}appending '{v}'")
                ret_value.append(v)
                if split_text != '':
                    if detail_log:
                        logging.debug(f"{log_prefix}Split test is not empty -> appending {split_text}")
                    ret_value.append(split_text)
            if detail_log:
                logging.debug(f"{log_prefix}OUT: '{ret_value}'")
            return ret_value, left

        (return_value, left) = process_tag(self.element, DOC_WIDTH, 0, 0, -1)

        new_list = []
        for i in range(len(return_value)):
            item = return_value[i]
            if item == UNDO_PREVIOUS_SPACE:
                continue
            if i == len(return_value) - 1:
               new_list.append(item)
            else:
                next_item = return_value[i+1]
                if next_item == UNDO_PREVIOUS_SPACE and item == ' ':
                    if detail_log:
                        logging.debug(f"Found UNDO_PREVIOUS_SPACE for a space")
                else:
                    new_list.append(item)
        if detail_log:
            logging.debug(f"Parse tag result: {return_value}")
        return {'content': new_list, 'left': left}


    def __str__(self):
        """Textual representation of the div."""
        attrs = {}
        for a in self.attributes:
            attrs[a] = getattr(self, a)
        ret_value = f"Div: {self.text} | {attrs}"
        return ret_value


    def __eq__(self, other):
        return (
            (self.page_no == other.page_no) 
            and (self.bottom == other.bottom)
            and (self.left == other.left)
            )


    def __ne__(self, other):
        return not (self == other)


    def __lt__(self, other):
        return (
            self.page_no < other.page_no
            or 
            (
                self.page_no == other.page_no 
                and self.bottom > other.bottom
            )
            or 
            (
                self.page_no == other.page_no 
                and self.bottom == other.bottom
                and self.left < other.left
            )
        )



    def merge(divs):
        """
        font_size
        bottom
        page_no
        left
        """
        font_size = sum([d.font_size for d in divs]) / len(divs)
        bottom = min([d.bottom for d in divs])
        page_no = divs[0].page_no
        left = min([d.left for d in divs])
        text = ' '.join([d.text.strip() for d in divs])
        d = Div(None)
        d.text = textlib.clean_text(text)
        d.setattr('font_size', font_size)
        d.setattr('bottom', bottom)
        d.setattr('page_no', page_no)
        d.setattr('left', left)
        return d



class DivFactory:
    """Factory for div elements."""

    classes_to_include = ('x', 'y', 'fs')

    def __init__(self, css_interpreter):
        self.css_interpreter = css_interpreter


    def build_divs(self, elements) -> list:
        """Get divs of the elements."""
        return_value = []
        page_divs = [e for e in elements if "data-page-no" in e.attrs]
        for page_div in page_divs:
            page_no = int(page_div.attrs["data-page-no"], base=16)
            for sub_div in page_div.find_all('div'):
                div = self.__build_div(sub_div, page_no)
                return_value.append(div)
        return return_value

    
    def __build_div(self, element, page_no) -> Div:
        """
        t (position), 
        m, 
        x (left), 
        y (bottom), 
        ff (font-family), 
        fs (font-size), 
        fc, 
        sc (screen), 
        ks, 
        ws (word-spacing),
        _<getal> (width)
        """
        div = Div(element)
        if "class" in element.attrs:          
            for c in element.attrs["class"]:
                if c.startswith(DivFactory.classes_to_include):
                    css_values = self.css_interpreter.get(c)
                    for k, v in css_values.items():
                        div.setattr(k.replace('-', '_'), v)
                    div.setattr('page_no', page_no)
        return div


class Section:

    def __init__(self, name, divs):
        self.name = name
        self.divs = divs


    def __get_content(self, css_interpreter) -> list:
        """returns List with dictionaries"""
        ret_val = []
        prev_page_no = 0
        prev_bottom = 0
        row = {'content': [], 'left': DOC_WIDTH}
        div_res = None
        for d in sorted(self.divs):
            if not (d.page_no == prev_page_no and d.bottom == prev_bottom):
                # Close old row (only if it contains content) and create a new one
                if len(row['content']) > 0:
                    ret_val.append(row)
                row = {'content': [], 'left': DOC_WIDTH}
            div_res = d.parse(css_interpreter)
            if len(div_res['content']) > 0:
                row['content'].append(div_res['content'])
            if div_res['left'] < row['left']:
                row['left'] = div_res['left']
            prev_page_no = d.page_no
            prev_bottom = d.bottom
        if div_res is not None and len(div_res['content']) > 0:
            ret_val.append(row)
        return ret_val


    def __get_column_count(self, content) -> int:
        number_of_rows = len(content)
        number_of_rows_with_splits = 0
        number_of_splits = 0
        for content_row in content:
            splits_in_row = 0
            splits_in_row += len(content_row) - 1
            for part in content_row:
                splits_in_row += part.count(SPLIT_VALUE)
            if splits_in_row > 0:
                number_of_rows_with_splits += 1
            number_of_splits += splits_in_row
        logging.debug(f"Total rows of raw data in section: {number_of_rows}")
        logging.debug(f"Total rows with splits: {number_of_rows_with_splits}")
        logging.debug(f"Total splits: {number_of_splits}")
        # Determine the number of columns:
        num_columns = int((number_of_splits+number_of_rows_with_splits) / number_of_rows_with_splits)
        logging.debug(f"Number of columns assumed: {num_columns}")
        return num_columns


    def __clean_rows(self, content_in, number_of_columns) -> list:
        cln_content = []
        for sub_content in content_in:
            row = self.__clean_single_row(
                sub_content['content'], 
                sub_content['left'], 
                number_of_columns
                )
            cln_content.append(row)
        return cln_content


    def __add_splits_to_row(
            self, content_in, left, number_of_columns, number_of_splits_in_row
        ) -> list:
        if number_of_splits_in_row > 0:
            raise NotImplementedError
        return_value = []
        # logging.debug(f"Need to add splits")
        columns_range = float(DOC_WIDTH) / number_of_columns
        # logging.debug(f"Approximate columns range: {columns_range}")
        split_locations = [i*columns_range for i in range(number_of_columns)]
        # logging.debug(f"split_locations: {split_locations}")
        diffs = [abs(Decimal(i)-left) for i in split_locations]
        # logging.debug(f"diffs: {diffs}")
        min_diff = min(diffs)
        # logging.debug(f"min_diff: {min_diff}")
        # zero-based column number where the value should be
        column_number_of_value = diffs.index(min_diff)
        # logging.debug(f"column_number_of_value: {column_number_of_value}")
        for i in range(number_of_columns):
            if i == column_number_of_value:
                return_value.extend(content_in)
            else:
                return_value.append(SPLIT_VALUE)
        return return_value


    def __clean_single_row(self, content_in, left, number_of_columns) -> list:
        # logging.debug(f"Clean row input: {content_in}")

        number_of_splits_assumed = number_of_columns - 1
        row_with_split = []
        for i in range(len(content_in)):
            row_with_split.extend(content_in[i])
            if i < len(content_in)-1:
                row_with_split.append(SPLIT_VALUE)

        number_of_splits_in_row = row_with_split.count(SPLIT_VALUE)
        if number_of_splits_in_row < number_of_splits_assumed:
            row_with_split = self.__add_splits_to_row(
                content_in, left, number_of_columns, number_of_splits_in_row
                )
        # logging.debug(f"row_with_split: {row_with_split}")

        merged_list_parts = []
        part_value = ''
        for row_part in row_with_split:
            if row_part == SPLIT_VALUE:
                merged_list_parts.append(textlib.clean_text(part_value))
                part_value = ''
            else:
                if isinstance(row_part, list):
                    for p in row_part:
                        part_value += p
                else:
                    part_value += row_part
        merged_list_parts.append(textlib.clean_text(part_value))
        # logging.debug(f"Clean row output: {merged_list_parts}")
        return merged_list_parts


    def parse(self, css_interpreter) -> list:
        logging.debug(f"Parsing section: {self}")
        return_value = []
        content = self.__get_content(css_interpreter)
        if len(content) == 0:
            logging.warning(f"Unable to find content for {self}")
            return None
        number_of_columns = self.__get_column_count(content)
        clean_rows = self.__clean_rows(content, number_of_columns)
        row_affiliation = self.__row_affiliation(clean_rows)
        contains_affiliation = row_affiliation > -1
        logging.debug(f"Section has affiliation? {contains_affiliation}")
        # als wel, regels uitlijnen
        if contains_affiliation:
            alligned_rows = self.__align_rows(clean_rows)
            for r in alligned_rows:
                d = {'section': self.name.strip() }
                for i in range(len(r)):
                    if i == row_affiliation:
                        d["affiliation"] = r[i]
                    else:
                        d["person"] = r[i]
                return_value.append(d)
                logging.debug(f"{d}")
        else:
            # als niet, gewoon alles uitpoepen
            for r in clean_rows:
                for p in r:
                    d = {'section': self.name.strip(), 'person': p }
                    return_value.append(d)
        logging.debug(f"Parsing section result: {len(return_value)} items")
        return return_value


    def __align_rows(self, rows) -> list:
        logging.debug("Aligning rows")
        return_value = [] # rows to return
        input_rows_length = len(rows)
        logging.debug(f"- Number of rows in: {input_rows_length}")
        last_processed_row = []
        for i in range(input_rows_length):
            row_in_process = rows[i]
            if i == 0:
                last_processed_row = row_in_process
            else:
                # als er geen None in voorkomen:
                if None not in row_in_process:
                    # last_processed_row door schuiven
                    return_value.append(last_processed_row)
                    last_processed_row = row_in_process
                else:
                    logging.debug(f"Merging: {row_in_process} INTO {last_processed_row}")
                    for ci in range(len(row_in_process)):
                        value = row_in_process[ci]
                        if value is not None:
                            last_processed_row[ci] += ' ' + value
        return_value.append(last_processed_row)
        logging.debug(f"- Number of rows in: {len(return_value)}")
        return return_value

        
    def __row_affiliation(self, rows) -> int:
        return_value = -1
        comma_count = {}
        for row in rows:
            for i in range(len(row)):
                if str(i) not in comma_count:
                    comma_count[str(i)] = 0
                if row[i] is not None and ',' in row[i]:
                    comma_count[str(i)] += 1
        logging.debug(f"comma_count: {comma_count}")
        highest_prob = 0.0
        for k, v in comma_count.items():
            prob = v / float(len(rows))
            if prob > AFFILIATION_COMMA_TRESHOLD:
                if prob > highest_prob:
                    return_value = int(k)
        return return_value


    def __str__(self):
        return f"Section: {self.name}, number_of_divs: {len(self.divs)}"


class Scope:

    def __init__(self, divs):
        self.divs = divs
        self.sections = []

    def add_section(self, section):
        # logging.debug(f"Appending {section} to scope")
        self.sections.append(section)

    def parse(self, css_interpreter) -> list:
        logging.debug(f"Parsing scope")
        content = []
        for section in self.sections:
            section_content = section.parse(css_interpreter)
            content.append(section_content)
        return content

    def __font_size_distribution(self):
        list_sizes = []
        for div in self.divs:
            if "font_size" in div.attributes:
                list_sizes.append((div.font_size, len(div.text.strip())))
        s = sorted(list_sizes)
        it = itertools.groupby(s, operator.itemgetter(0))
        for key, subiter in it:
            yield key, sum(item[1] for item in subiter)
        

    def most_used_font_size(self) -> Decimal:
        size_most_used = 0
        count_used = 0
        for x in list(self.__font_size_distribution()):
            if x[1] > count_used:
                count_used = x[1]
                size_most_used = x[0]
        return size_most_used


    def get_headers(self) -> list:
        return_value = []
        most_used_font_size = self.most_used_font_size()
        logging.debug(f"Most used font size: {most_used_font_size}")
        header_divs = sorted([d for d in self.divs if d.font_size > most_used_font_size])
        i = 0        
        merging_divs = []
        while i < len(header_divs):
            current = header_divs[i]
            logging.debug(f"Processing possible header: {current}")
            merging = True
            
            merging_divs.append(current)

            if len(header_divs) == i+1:
                logging.debug(f"Decision: not merging because it is the last one")
                merging = False
            else:
                next = header_divs[i+1]
                logging.debug(f"Next possible header: {next}")
                bottom_diff = current.bottom - next.bottom
                if bottom_diff > most_used_font_size:
                    merging = False
                    logging.debug(f"Decision: not merging because bottom_diff > most_used_font_size")
                else:
                    logging.debug(f"Decision: merging because bottom_diff =< most_used_font_size")

            if not merging:
                if len(merging_divs) == 1:
                    return_value.append(merging_divs[0])
                else:
                    new_div = Div.merge(merging_divs)
                    return_value.append(new_div)
                merging_divs = []
                
            i += 1
        logging.debug(f"Resulting headers:")
        for i in return_value:
            logging.debug(f"{i}")

        return return_value


class DocumentParser:


    def __init__(self):
        self.divs = []
        """All divs in the document"""


    def parse(self, file) -> list:
        """Gets the content."""
        soup = self.__get_html_model(file)
        css_interpreter = self.__css_interpreter(soup)
        div_factory = DivFactory(css_interpreter)
        self.divs = div_factory.build_divs(soup.find_all('div'))
        (scope_start_div, scope_end_div) = self.__get_range(soup)
        divs_inscope = self.__divs_between(scope_start_div, scope_end_div)
        scope = Scope(divs_inscope)
        headers = sorted(scope.get_headers())
        for hi in range(len(headers)):
            start_div = headers[hi]
            end_div = scope_end_div
            if hi < len(headers) - 1:
                end_div = headers[hi+1]
            divs = self.__divs_between(start_div, end_div, scope.most_used_font_size())
            section = Section(headers[hi].text, divs)
            scope.add_section(section)
        content = scope.parse(css_interpreter)
        logging.debug("End parsing")
        return content


    def __divs_between(self, start_div, end_div, font_size = None):
        return_value = []
        for div in self.divs:
            if (
                all(x in div.attributes for x in ["bottom", "page_no"])
                and div > start_div
                and div < end_div
            ):
                if font_size is not None:
                    if div.font_size == font_size:
                        return_value.append(div)
                else:
                    return_value.append(div)
        return return_value


    
    def __css_interpreter(self, soup) -> CssInterpreter:
        """Get a css interpreter from the soup."""
        stylesheets = []
        for styletag in soup.findAll('style', type='text/css'):
            if styletag.string:
                stylesheets.append(styletag.string)
        css_interpreter = CssInterpreter()
        css_interpreter.build(stylesheets)
        return css_interpreter


    def __get_range(self, soup) -> Tuple:
        """Search for the range of interesting data. Returns tuple with divs"""
        div_with_bottom = [div for div in self.divs if "bottom" in div.attributes]
        start_div = None
        end_div = None
        for div in div_with_bottom:
            if div.text.strip() == 'Organization':
                start_div = div
                logging.debug(f"Found start tag: {div} ({div.text})")
                break
        if start_div is None:
            logging.error("Unable to find a range start")
        for div in div_with_bottom:
            if div > start_div:
                if div.font_size >= start_div.font_size:
                    end_div = div
                    logging.debug(f"Found end tag: {div} ({div.text})")
                    break
        if end_div is None:
            logging.error("Unable to find a range end")
            raise ParseError("Unable to find a range end")
        return (start_div, end_div)
        

    def __get_html_model(self, file) -> BeautifulSoup:
        """Get HTML model from the file."""
        f=codecs.open(file, 'r', 'utf-8')
        content = f.read()
        soup = BeautifulSoup(content, 'html.parser')
        return soup


if __name__ == '__main__':
    logging.basicConfig(level=logging.DEBUG)
    # x = 'software\\lncs\\front_matter_reader\\examples\\pdf2htmlpx\options\\2021_Bookmatter_ApplicationsOfEvolutionaryComp.html'
    # x = 'pdf2htmlex_out/1.html'
    x = 'pdf2htmlex_out/2021_Bookmatter_ApplicationsOfEvolutionaryComp.html'
    # x = 'pdf2htmlex_out/2021_Bookmatter_CoordinationOrganizationsInsti.html'

    dp = DocumentParser()
    content = dp.parse(x)
    
