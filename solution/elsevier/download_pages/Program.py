import requests
import logging
import codecs
from bs4 import BeautifulSoup
import time
import datetime
from database import Postgress, Saver
import json
import configparser
from selenium.webdriver.firefox.webdriver import WebDriver
from stem import Signal
from stem.control import Controller
from selenium import webdriver
from selenium.webdriver.firefox.options import Options

logging.basicConfig(level=logging.INFO, format='%(name)s - %(levelname)s - %(message)s')

# schema_name = 'opencitations_api'

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







# def get_current_ip():
#     session = requests.session()

#     session.proxies = {}
#     session.proxies['http']='socks5h://localhost:9050'
#     session.proxies['https']='socks5h://localhost:9050'

#     try:
#         r = session.get('http://httpbin.org/ip')
#     except Exception as e:
#         logging.debug(str(e))
#     else:
#         return r.text


def renew_tor_ip():
    with Controller.from_port(port = 9051) as controller:
        controller.authenticate(password=config['TOR_PASSWORD'])
        controller.signal(Signal.NEWNYM)


# def get_article_html_page(url: str) -> str:
#     response = requests.get(url)
#     return response.text


def get_search_result_page(issn: str, show: int, offset:int) -> str:   
    url = f"https://www.sciencedirect.com/search?docId={issn}&show={show}&offset={offset}"
    return url
#     session = requests.session()

#     session.proxies = {}
#     session.proxies['http']='socks5h://localhost:9050'
#     session.proxies['https']='socks5h://localhost:9050'

#     try:
#         r = session.get(url)
#     except Exception as e:
#         logging.debug(str(e))
#     else:
#         logging.info(r.status_code)
#         logging.info(r.url)
#         return r.text


# def get_article_links(content: str) -> list:
#     return_value = []
#     soup = BeautifulSoup(content, 'html.parser')
#     links = soup.findAll("a", _class="result_list_title_link")


def get_driver() -> WebDriver:
    logging.info("Getting driver")
    options = Options()
    # options.headless = True
    profile = webdriver.FirefoxProfile()
    profile.set_preference('network.proxy.type', 1)
    profile.set_preference('network.proxy.socks', 'localhost')
    profile.set_preference('network.proxy.socks_port', 9050)
    profile.set_preference("general.useragent.override", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36")
    driver = webdriver.Firefox(profile, options=options)
    driver.delete_all_cookies()
    return driver


if __name__ == '__main__':
    logging.info("Starting program")
    driver = get_driver()
    url = get_search_result_page('2772-5596', 25, 0)
    driver.get(url)

    content = driver.page_source


    driver.close()


    # logging.info(f"Renewing ip")
    # renew_tor_ip()
    # time.sleep(5)
    # ip = get_current_ip()
    # logging.info(f"new ip: {ip}")
    # content = get_search_result_page('2772-5596', 25, 0)
    # get_article_links(content)

    # # url = f"https://www.sciencedirect.com/science/article/pii/S2772559621000079"
    # # html = get_article_html_page(url)
    logging.info("Program finished")
