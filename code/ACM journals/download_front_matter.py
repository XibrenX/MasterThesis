import csv
import logging
import os

import sys
sys.path.append('../') #Allow importing from base directory

import tor
from read_config import config

outdir = config['ACM_FRONT_MATTER_OUTPUT']
datadir = config['ACM_DATA_DIR']

logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', handlers=[
    logging.FileHandler(outdir + '/front_matter_log.txt'),
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

    # CSV export query: SELECT journal_code, volume, issue, front_matter FROM issue_pages WHERE front_matter IS NOT NULL, exported from Postgres with HeidiSQL
    with open(datadir + '/front_matters_export.csv') as f:
        reader = csv.reader(f)
        first = True
        for row in reader:
            if len(row) > 0 and not first: #Skip the empty line
                download_front_matter(row[0], row[1], row[2], row[3])
                count += 1
            first = False

def download_front_matter(journal_code, volume, issue, url):
    global count

    out_path = outdir + '/' + '_'.join([journal_code, volume, issue]) + '.pdf'

    if (os.path.exists(out_path)):
        logging.info(f'Skipping already downloaded {url}')
        return

    logging.info(f'Downloading {url}')

    try:
        result = req.get_session().get(url, allow_redirects=True)
        if result.status_code == 200:
            os.makedirs(os.path.dirname(out_path), exist_ok=True)
            with open(out_path, 'wb') as fp:
                fp.write(result.content)
            logging.info(f'Content written to {out_path}')
        elif result.status_code == 404:
            logging.info("Not found")
        elif result.status_code == 403:
            logging.error(f'BANNED! After {count}')
            exit()
        else:
            raise Exception(f'Status code not ok for {url} - {result.status_code}')
    except Exception as Argument:
        logging.exception(f'Error while requesting issue {url}')

if __name__== "__main__":
    main()