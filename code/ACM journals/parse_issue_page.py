
import csv
import logging
import os
import re
import json

from bs4 import BeautifulSoup
from database import Postgress, Saver

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

site = 'https://dl.acm.org'

outdir = config['ACM_ISSUE_PAGES_OUTPUT']
datadir = config['ACM_DATA_DIR']

logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', handlers=[
    logging.FileHandler(outdir + '/parse_log.txt'),
    logging.StreamHandler()
])

db = Postgress(
    server=config['POSTGRES_SERVER'], 
    database=config['POSTGRES_DB'],
    user=config['POSTGRES_USER'],
    password=config['POSTGRES_PASSWORD']
    )
saver = Saver(db)

def main():
    with open(datadir + '/journals.csv') as f:
        reader = csv.reader(f)
        issues = []
        first = True
        for row in reader:
            if len(row) > 0 and not first: #Skip the empty line
                code = row[0]
                url = f'{site}/toc/{code}/current'
                while url is not None:
                    content = get_issue(url)
                    url, issue = parse_issue(url, code, content)
                    if issue is not None:
                        issues.append(issue)
            first = False

        # Extract editors from issues to store it in a seperate table
        # Extract editorials from issues to store it in a seperate table
        editors = []
        editorials = []
        for issue in issues:
            i_editors = issue.get('editors')
            if i_editors:
                for i_editor in i_editors:
                    i_editor['key'] = issue['key']
                    editors.append(i_editor)
                del issue['editors']
            
            i_editorials = issue.get('editorial')
            if i_editorials:
                for i_editorial in i_editorials:
                    i_editorial['key'] = issue['key']
                    editorials.append(i_editorial)
                del issue['editorial']

            i_publisher = issue.get('publisher')
            if i_publisher:
                del issue['publisher']
                issue.update(i_publisher)

        saver.save('acm_w', 'issue_pages', json_fy_list_dict(issues))
        saver.save('acm_w', 'issue_editors', json_fy_list_dict(editors))
        saver.save('acm_w', 'issue_editorials', json_fy_list_dict(editorials))

def get_issue(url: str) -> str:
    out_path = outdir + url[len(site):]
    if not out_path.endswith(".html"):
        out_path += '.html'
    
    if (os.path.exists(out_path)):
        logging.info(f'Reading {out_path}')
        with open(out_path) as fp:
            return fp.read()
    else:
        logging.warning(f'Not found {out_path}')
    return None

def parse_issue(url: str, code: str, content: str or None) -> any:
    if content is None:
        return None, None

    soup = BeautifulSoup(content, 'html.parser')
    issue = {}
    issue['url'] = url
    issue['journal_code'] = code
    issue['journal_title'] = soup.find(['span', 'h1'], class_='title').text

    issue_infos = soup.find(class_='item-meta').find(class_='left-bordered-title').find_all('span')
    for i in range(len(issue_infos)):
        text = issue_infos[i].text.lower()

        if issue_infos[i].has_attr('class'):
            c = issue_infos[i].get('class')
            if 'date' in c:
                if not 'date' in issue:
                    issue['date'] = text
                continue
            elif 'in-progress':
                if text == 'issue-in-progress':
                    issue['in_progress'] = 'true'
                    continue
                elif text == 'current issue':
                    continue #OK Do nothing

        if 'volume' in text:
            volume_num = try_match('\d+', text)
            if volume_num.startswith(', volume '):
                volume_num = volume_num[len(', volume '):]
            issue['volume'] = volume_num
        elif 'issue' in text:
            issue_num = try_match('\d+', text)
            if issue_num.startswith(', issue '):
                issue_num = issue_num[len(', issue '):]
            issue['issue'] = issue_num
        else:
            logging.warning(f"Could not parse {text} as issue info")

    meta_infos = soup.find(class_='item-meta__info').find_all(class_='item-meta-row')
    for meta_info in meta_infos:
        if meta_info.find('strong', text='Publisher:'):
            publisher = {}
            for div in meta_info.find(class_='item-meta-row__value'):
                if div.get('class') == ['published-info']:
                    l = div.find(class_='rlist--inline comma')
                    if l:
                        publisher_info = []
                        for li in l.find_all('li'):
                            publisher_info.append(li.text)
                        publisher['publisher'] = publisher_info
                    else:
                        publisher['publisher'] = div.text
                elif div.find(text='ISSN:'):
                    publisher['issn'] = div.find(text='ISSN:').next.text
                elif div.find(text='EISSN:'):
                    publisher['eissn'] = div.find(text='EISSN:').next.text
                else:
                    logging.warning(f"Could not parse {div} as publisher info")

            issue['publisher'] = publisher
        elif meta_info.find('strong', text='Editor:'):
            editors = []
            for editor_li in meta_info.find(class_='item-meta-row__value').find_all('li'):
                editor = parse_editor(editor_li)
                editors.append(editor)
            issue['editors'] = editors
        elif meta_info.find('strong', text='Tags:'):
            continue
        else:
            logging.warning(f"Could not parse {meta_info} as meta info")

    content = soup.find(class_='table-of-content')

    front_matter = content.find('a', title=re.compile('^front\s?matter', re.IGNORECASE))
    if not front_matter:
        front_matter = content.find('a', title=re.compile('masthead', re.IGNORECASE))
    if not front_matter:
        front_matter = content.find('a', title=re.compile('ed(itorial)?\s?board', re.IGNORECASE))

    if front_matter:
        issue['front_matter'] = parse_link(front_matter.get('href'))

    issue['editorial'] = []

    issue_items = content.find_all(class_='issue-item-container')
    for issue_item in issue_items:
        title = issue_item.find(class_='issue-item__title')

        if title is None:
            logging.warning(f"Could not find title of issue item {issue_item}")
            continue

        title_text = title.text

        heading = issue_item.find(class_='issue-heading')
        if heading is None:
            heading_text = ''
        else:
            heading_text = heading.text

        if 'reviewers' in title_text.lower() or 'editor' in title_text.lower() or 'editorial' in heading_text:
            editorial = {}
            
            editorial['title'] = title_text
            editorial['link'] = parse_link(title.find('a').get('href'))
            editorial['authors'] = []

            for editor_ul in issue_item.find_all('ul', {'aria-label': 'authors'}):
                for editor_li in editor_ul.find_all('li'):
                    editor = parse_editor(editor_li)
                    editorial['authors'].append(editor)

            issue['editorial'].append(editorial)

    doia = soup.find('a', {'data-doi':True})
    if doia:
        doi = doia.get('data-doi')
        issue['doi'] = f'https://doi.org/{doi}'

    previous_link = soup.find('a',class_='content-navigation__btn--pre')
    if previous_link:
        previous_link = parse_link(previous_link.get('href'))

        if previous_link.startswith('http'):
            logging.info(f'Previous link found: {previous_link}')
        elif not previous_link.startswith('javascript:'):
            logging.warning(f"Could not parse {previous_link} as previous link")
            previous_link = None
        else:
            previous_link = None
    else:
        logging.warning(f"Could not find previous link")

    issue['previous_link'] = previous_link

    issue['key'] = '/'.join([p for p in [issue.get('journal_code'), issue.get('volume'), issue.get('issue')] if p is not None])

    logging.info(issue)

    return previous_link, issue

def json_fy_list_dict(items: list):
    return [json_fy_dict(item) for item in items]

def json_fy_dict(item: dict):
    o_item = {}
    for key, value in item.items():
        if isinstance(value, list) and len(value) == 0:
            value = None

        if value is not None:
            if not isinstance(value, str):
                value = json.dumps(value)

            o_item[key] = value
    return o_item

def parse_editor(editor_li):
    editor = {}

    a = editor_li.find('a')
                
    if a:
        editor['link'] = parse_link(a.get('href'))
        editor['name'] = a.text
    else:
        editor['name'] = editor_li.text

    inst = editor_li.find(class_='loa_author_inst')
    if inst:
        editor['inst'] = inst.text
        
    return editor

def parse_link(link):
    if link.startswith('/'):
        link = site + link
    return link

def try_match(pattern, s):
    match = re.search(pattern, s)
    if match is not None:
        return match.group()
    else:
        return s

if __name__== "__main__":
    main()