import requests
import logging
import codecs
from bs4 import BeautifulSoup
import time
import datetime
import database
import saver
import pyodbc
import re

server = 'localhost'
db_name = 'study'
driver = '{ODBC Driver 17 for SQL Server}'
conn_str = f'Driver={driver};Server={server};Database={db_name};Trusted_Connection=yes;'
schema_name = 'core'

db = database.SqlServer(conn_str)
saver = saver.Saver(db)

def parse(content, dts, url) -> list:
    
    soup = BeautifulSoup(content, 'html.parser')
    titles = [t.find('b').text for t in soup.find_all('th')]
    evenrows = soup.find_all('tr', class_='evenrow')
    oddrows = soup.find_all('tr', class_='oddrow')
    data = []
    for row in evenrows + oddrows:
        v = {t: None for t in titles}
        # print(row)
        attributes = row.find_all('td')
        i = 0
        for a in attributes:
            value = a.text.strip()
            if a.text.strip() == 'view':
                value = a.find('a')['href']
            v[titles[i]] = value
            i = i + 1
        v["$_extract_dts"] = timestamp
        v["$_rec_src"] = url
        data.append(v)
    return data
    

if __name__ == '__main__':
    types = ['jnl-ranks', 'conf-ranks']
    for t in types:
        print(f"Working on {t}")
        for i in range(1, 20):
            print(f"site: {i}")
            url = f"http://portal.core.edu.au/{t}/?search=&by=all&source=CORE2020&sort=atitle&page={i}"
            timestamp = str(datetime.datetime.now())
            response = requests.get(url)
            if response.status_code != 200:
                print(f"Got {response.status_code}: stopping script")
                break
            data = parse(response.content, timestamp, url)
            saver.save('core', t, data)
            time.sleep(1)
    print(f"done")

