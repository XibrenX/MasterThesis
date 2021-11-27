import requests
import logging
import codecs
from bs4 import BeautifulSoup
import time
import datetime
from database import Postgress, Saver
import json
import configparser
from stem import Signal
from stem.control import Controller


logging.basicConfig(level=logging.INFO, format='%(name)s - %(levelname)s - %(message)s')

schema_name = 'opencitations_api'

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


def execute_request(query) -> list:
    url = f"https://opencitations.net/index/api/v1/references/{query}"
    response = requests.get(url)
    return response.json()


def get_queries() -> list:
    logging.info("Fetch queries from database")
    query = f"select REPLACE(doi, 'https://doi.org/', '') as doi from springer_lncs.chapter"
    query_result = db.execute_query_result(query)
    result = [x['doi'] for x in query_result]
    return result


def get_current_ip():
    session = requests.session()

    session.proxies = {}
    session.proxies['http']='socks5h://localhost:9050'
    session.proxies['https']='socks5h://localhost:9050'

    try:
        r = session.get('http://httpbin.org/ip')
    except Exception as e:
        logging.debug(str(e))
    else:
        return r.text


def renew_tor_ip():
    with Controller.from_port(port = 9051) as controller:
        controller.authenticate(password=config['TOR_PASSWORD'])
        controller.signal(Signal.NEWNYM)



def main():
    logging.info("Program started")
    queries = get_queries()
    total_queries = len(queries)
    logging.info(f"Total queries to do: {total_queries}")
    i = 1
    logging.info(f"Renewing ip")
    renew_tor_ip()
    time.sleep(5)
    ip = get_current_ip()
    logging.info(f"new ip: {ip}")
    for q in queries:
        dt = execute_request(q)
        saver.save(schema_name, 'reference', dt)
        i = i + 1
        if i % 100 == 0:
            logging.info(f"Query {i} / {total_queries}")
    logging.info("Done")


if __name__ == '__main__':
    main()
