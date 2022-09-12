import csv
from dataclasses import replace

from datetime import datetime
from dateutil.relativedelta import relativedelta

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

# Python script filters all issue pages for dates >= 01-01-2020
filter_date = datetime.strptime('01-01-2020', '%d-%M-%Y')

def main():
    with open(config['ACM_DATA_DIR'] + '/issue_pages_export.csv') as fr: # Dump of issue pages table in Postgres, csv file dump created with HeidiSQL, columns are key, journal_code, volume, issue, date
        reader = csv.DictReader(fr)

        with open(config['ACM_DATA_DIR'] + '/issues_selected.csv', 'w', newline='') as fw:
            writer = csv.writer(fw)
            writer.writerow(['key', 'journal_code', 'volume', 'issue', 'date', 'date_parsed'])

            for row in reader:
                datestr = row['date'].replace('.', '').replace(',', '').replace('  ', ' ').replace('sept ', 'sep ')
                split = datestr.split(' ')
                if len(split) == 1:
                    year = process_year(split[0])
                    month = 'jan'
                    day = '1'
                elif len(split) == 2:
                    year = process_year(split[1])
                    month = process_month(split[0])
                    day = '1'
                else:
                    year = process_year(split[2])
                    month = process_month(split[1])
                    day = split[0]

                date = get_date(day, month, year)

                if date >= filter_date:
                    writer.writerow([row['key'], row['journal_code'], row['volume'], row['issue'], row['date'], date.strftime('%d-%m-%Y')])

def process_year(year) -> str:
    yearint = int(year)
    if yearint < 100:
        if yearint <= datetime.now().year - 2000:
            return '20' + year
        else:
            return '19' + year
    else:
        return year

def process_month(month) -> str:
    #what the hack is octember https://dl.acm.org/toc/taslp/2015/23/10
    month = month.replace('summer', 'july').replace('fall', 'october').replace('octember', 'october').replace('/', '-').replace('–','-')
    if '-' in month: #When issue has multiple months we take the last
        month = month.split('-')[-1]
    return month

def get_date(day, month, year) -> datetime:
    if len(month) > 3:
        return datetime.strptime(' '.join([day, month, year]), '%d %B %Y') #short month name: apr
    else:
        return datetime.strptime(' '.join([day, month, year]), '%d %b %Y') #long month name: April

if __name__== "__main__":
    main()