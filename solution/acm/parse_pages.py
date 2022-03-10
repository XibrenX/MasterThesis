from bs4.element import NavigableString
import logging
from bs4 import BeautifulSoup
from database import Postgress, Saver
import re
import configparser
import os

# Directory with HTML pages
EDITORIAL_BOARD_HTML_DIR = "solution/acm/editorial_boards/"
# Schema where the resulting tables should be placed
ACM_DATABASE_SCHEMA = "acm"

logging.basicConfig(level=logging.INFO, format='%(name)s - %(levelname)s - %(message)s')

def read_config(path) -> configparser.SectionProxy:
    logging.info('Reading configuration')
    with open(path, 'r') as f:
        config_string = '[SECTION]\n' + f.read()
    config = configparser.ConfigParser()
    config.read_string(config_string)
    return config['SECTION']


config = read_config('solution/config')
db = Postgress(
    server=config['POSTGRES_SERVER'], 
    database=config['POSTGRES_DB'],
    user=config['POSTGRES_USER'],
    password=config['POSTGRES_PASSWORD']
    )
saver = Saver(db)


def main():
    logging.info("main")
    for filename in os.listdir(EDITORIAL_BOARD_HTML_DIR):
        if filename.endswith(".html"):
            logging.info(f"processing {filename}")
            with open(dir + filename) as fp:
                soup = BeautifulSoup(fp, 'html.parser')
                read_content(soup)


def read_content(soup: BeautifulSoup):
    profiles = []
    title_element = soup.find(class_='title')
    journal = clean_text(title_element.text)
    logging.info(f"journal: {journal}")
    sections = soup.find_all(class_="section__title")
    for section_element in sections:
        role = clean_text(section_element.text)
        logging.info(f"role: {role}")
        start_elem = section_element
        for row in get_rows(start_elem):
            for profile_element in row.find_all('div', class_='profile-meta'):
                p = process_profile(profile_element, role=role, journal=journal)
                profiles.append(p)

    saver.save(ACM_DATABASE_SCHEMA, 'editorial_boards', profiles)


def clean_text(input: str) -> str:
    output = re.sub(' +', ' ', input.replace("\n", " ")).strip()
    return output


def get_rows(start_element) -> list:
    rows = []
    start_elem = start_element
    for elem in start_elem.next_siblings:
        if type(elem) is NavigableString:
            continue
        if elem and elem.name == 'div' and elem.has_attr('class') and elem['class'][0] == 'row':
            rows.append(elem)
            start_elem = elem
        if elem and elem.has_attr('class') and elem['class'][0] == 'section__title':
            break
    return rows
        

def process_profile(profile_element, role: str, journal:str) -> dict:

    name_element = profile_element.find('h4', class_='item-meta-row')
    affiliation_element = name_element.findNext('div', class_='item-meta-row')
    country_element = name_element.findNext('em')

    profile = {
        'role': role,
        'journal': journal,
        'name': clean_text(name_element.text),
        'affiliation': clean_text(affiliation_element.text),
    }
    if country_element:
        profile['country'] = clean_text(country_element.text)

    logging.info(profile)
    return profile



if __name__ == '__main__':
    main()