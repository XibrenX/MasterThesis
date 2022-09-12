#Code from Ewoud "\IM9906\solution\acm\download_editorial_board_pages.py", modified by Wibren to read csv of the parse_journals_overview_page.py
import logging
import csv
from os import path

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config
import tor

logging.basicConfig(level=logging.DEBUG, format='%(name)s - %(levelname)s - %(message)s')

start_num = 0

datadir = config['ACM_DATA_DIR']

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

def main():
    """
    Reads input file.
    """
    with open(datadir + '/journals.csv') as f:
        reader = csv.reader(f)
        isFirst = True
        for row in reader:
            if isFirst:
                isFirst = False
            elif len(row) > 0: #Skip the empty line
                if not path.exists(f'{datadir}/editorial_boards/{row[0]}.html'): #skip existing files (for retry)
                    process_link(row[2], row[0])

def process_link(link: str, code: str):
    """
    Downloads HTML page of a link.
    """
    logging.debug(link)
    try:
        editors_url = link + '/editorial-board'
        editorial_board_content = req.get_session().get(editors_url)
        with open(f"{datadir}/editorial_boards/{code}.html", "w") as f:
            f.write(editorial_board_content.text)
    except Exception as e:
        logging.error(f"{code} failed")
        logging.exception(e)


if __name__ == '__main__':
    logging.debug('Program start')
    main()
    logging.debug('Program finished')
