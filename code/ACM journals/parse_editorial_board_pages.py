#Code from Ewoud "\IM9906\solution\acm\parse_pages.py"

from bs4.element import NavigableString
import logging
from bs4 import BeautifulSoup
from database import Postgress, Saver
import re
import os
from pathlib import Path

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

logging.basicConfig(level=logging.INFO, format='%(name)s - %(levelname)s - %(message)s')

db = Postgress(
    server=config['POSTGRES_SERVER'], 
    database=config['POSTGRES_DB'],
    user=config['POSTGRES_USER'],
    password=config['POSTGRES_PASSWORD']
    )
saver = Saver(db)

data_dir = config['ACM_DATA_DIR'] + '/editorial_boards/'

def main():
    logging.info("main")
    for filename in os.listdir(data_dir):
        if filename.endswith(".html"):
            logging.info(f"processing {filename}")
            with open(data_dir + filename) as fp:
                journal_code = Path(filename).stem
                soup = BeautifulSoup(fp, 'html.parser')
                read_content(soup, journal_code)


def read_content(soup: BeautifulSoup, journal_code: str):
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
                p = process_profile(profile_element, role=role, journal=journal, journal_code=journal_code)
                profiles.append(p)

    saver.save('acm_w', 'editorial_boards', profiles)


def clean_text(input: str) -> str:
    input = input.replace("\n", " ")
    input = re.sub(' +', ' ', input)
    input = input.strip()
    return input


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

def complete_url(url: str) -> str:
    if url.startswith('/'):
        url = 'https://dl.acm.org' + url
    return url

def process_profile(profile_element, role: str, journal:str, journal_code:str) -> dict:

    name_element = profile_element.find('h4', class_='item-meta-row')
    profile_link = None
    link_element = name_element.find('a')
    if link_element != None:
        profile_link = complete_url(link_element['href'])

    affiliation_element = name_element.findNext('div', class_='item-meta-row')
    country_element = name_element.findNext('em')

    websites = map(lambda a: a['href'], profile_element.find_all('a', {'data-title':'Website'}))

    profile = {
        'role': role,
        'journal': journal,
        'journal_code': journal_code,
        'name': clean_text(name_element.text),
        'profile_link': profile_link,
        'affiliation': clean_text(affiliation_element.text),
        'websites': '[' + ','.join(map(lambda w: '"' + w + '"', websites)) + ']'
    }
    if country_element:
        profile['country'] = clean_text(country_element.text)

    logging.info(profile)
    return profile



if __name__ == '__main__':
    main()