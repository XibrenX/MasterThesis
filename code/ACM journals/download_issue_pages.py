import csv
import logging
import os

from bs4 import BeautifulSoup

import sys
sys.path.append('../') #Allow importing from base directory

import tor
from read_config import config

site = 'https://dl.acm.org'

outdir = config['ACM_ISSUE_PAGES_OUTPUT']
datadir = config['ACM_DATA_DIR']

issue_pages_queue = []

logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', handlers=[
    logging.FileHandler(outdir + '/download_log.txt'),
    logging.StreamHandler()
])

req = tor.DelayLimiter(
    tor.SessionLimiter(
        tor.SessionDelayLimiter(
            tor.WithoutTor(),
            min_delay_sec=config.getint('WITHOUT_TOR_MIN_SESSION_DELAY_SEC'),
            max_delay_sec=config.getint('WITHOUT_TOR_MAX_SESSION_DELAY_SEC')
        ),
        min_refresh_interval=config.getint('WITHOUT_TOR_MIN_REFRESH_INTERVAL'),
        max_refresh_interval=config.getint('WITHOUT_TOR_MAX_REFRESH_INTERVAL'),
    ),
    min_delay_sec=config.getint('WITHOUT_TOR_MIN_DELAY_SEC'),
    max_delay_sec=config.getint('WITHOUT_TOR_MAX_DELAY_SEC')
)

count = 0

def main():
    global count

    logging.info('Starting main')
    read_issue_pages_queue()
    logging.info(f'Restored queue to {len(issue_pages_queue)}')

    while len(issue_pages_queue) > 0:
        write_issue_pages_queue()
        url = issue_pages_queue[0]
        content = get_issue(url)
        if content is not None:
            parse_issue(content, url)
        issue_pages_queue.pop(0)
        count += 1

issue_pages_file = outdir + '/issue_pages_queue.txt'

def write_issue_pages_queue():
    with open(issue_pages_file, 'w') as fp:
        fp.write('\n'.join(issue_pages_queue))

def read_issue_pages_queue():
    global issue_pages_queue

    if os.path.exists(issue_pages_file):
        with open(issue_pages_file) as fp:
            issue_pages_queue = fp.read().splitlines()
    else:
        with open(datadir + '/journals.csv') as f:
            reader = csv.reader(f)
            first = True
            for row in reader:
                if len(row) > 0 and not first: #Skip the empty line
                    issue_pages_queue.append(f'{site}/toc/{row[0]}/current')
                first = False

def get_issue(url: str) -> str:
    out_path = outdir + url[len(site):]
    if not out_path.endswith(".html"):
        out_path += '.html'

    if (os.path.exists(out_path)): # Skip already downloaded issues, but still parse previous_link
        logging.info(f'Reading {out_path}')
        with open(out_path) as fp:
            return fp.read()
    else:
        logging.info(f'Downloading {url}')
        try:
            result = req.get_session().get(url, allow_redirects=True)
            if result.status_code == 200:
                content = result.text
                os.makedirs(os.path.dirname(out_path), exist_ok=True)
                with open(out_path, 'w') as fp:
                    fp.write(content)
                logging.info("Content written")
                return content
            elif result.status_code == 404:
                logging.info("Not found")
                return None
            elif result.status_code == 403:
                logging.error(f'BANNED! After {count}')
                exit()
            else:
                raise Exception(f'Status code not ok for {url} - {result.status_code}')
        except Exception as Argument:
            logging.exception(f'Error while requesting issue {url}')
            return None
    
def parse_issue(content: str, url: str):
    soup = BeautifulSoup(content, 'html.parser')
    previous_link = soup.find('a',class_='content-navigation__btn--pre')
    if previous_link:
        previous_link = parse_link(previous_link.get('href'))

        if previous_link.startswith('http'):
            issue_pages_queue.append(previous_link)
            logging.info(f'Previous link found: {previous_link}')
        elif not previous_link.startswith('javascript:'):
            logging.warning(f"Could not parse {previous_link} as previous link")
    else:
        logging.warning(f"Could not find previous link")

def parse_link(link):
    if link.startswith('/'):
        link = site + link
    return link

if __name__== "__main__":
    #debug_tor()
    main()