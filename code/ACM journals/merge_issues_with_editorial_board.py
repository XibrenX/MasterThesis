import csv

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

def main():
    issues_by_journal = {}

    with open(config['ACM_DATA_DIR'] + '/issues_selected.csv', encoding="utf-8") as fi:
        issue_reader = csv.DictReader(fi)

        for issue in issue_reader:
            issues_by_journal.setdefault(issue['journal_code'], []).append(issue)

    with open(config['ACM_DATA_DIR'] + '/editorial_boards_export.csv', encoding="utf-8") as fe: # Export from editorial boards table in Postgres, exported to CSV with HeidiSQL
        editorial_board_reader = csv.DictReader(fe)

        not_found = {}

        with open(config['ACM_DATA_DIR'] + '/editorial_board_issue_merge.csv', 'w', newline='') as fw:
            writer = csv.writer(fw)
            writer.writerow(['wkey', 'journal_code', 'volume', 'issue', 'name', 'role'])

            for editorial_board_member in editorial_board_reader:
                journal_code = editorial_board_member['journal_code']

                if journal_code in issues_by_journal:
                    for issue in issues_by_journal[journal_code]:
                        w_key = '#' + issue['volume']
                        if issue['issue'] != '\\N':
                            w_key += '#' + issue['issue']

                        writer.writerow([w_key, issue['journal_code'], issue['volume'], issue['issue'], editorial_board_member['name'], editorial_board_member['role']])
                else:
                    not_found[journal_code] = not_found.get(journal_code, 0) + 1

        for key, value in not_found.items():
            print(f'could not find journal code {key} {value} times')

if __name__== "__main__":
    main()