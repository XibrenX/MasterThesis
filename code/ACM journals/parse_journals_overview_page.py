from bs4 import BeautifulSoup
import csv

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

def main():
    filename = config['ACM_DATA_DIR'] + '/ACM Journal.html' # Manually downloaded file from browser after complete loading all journals with JavaScript on page https://dl.acm.org/journals
    with open(filename) as fp:
        soup = BeautifulSoup(fp, 'html.parser')
        read_content(soup)

def read_content(soup):
    journals = soup.findAll(class_='search__item-journal')
    print(f'Journals: {len(journals)}')

    with open(config['ACM_DATA_DIR'] + '/journals.csv', 'w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        for journal in journals:
            a = journal.find(class_='browse-link')
            link = a['href']
            code = a.find(class_='browse-code').string
            title = a.find(class_='browse-title').string
            print(f'Journal {code}: {title}: {link}')
            writer.writerow([code, title, link])

if __name__== "__main__":
    main()