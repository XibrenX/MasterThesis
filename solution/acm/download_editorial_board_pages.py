import requests
import logging

logging.basicConfig(level=logging.DEBUG, format='%(name)s - %(levelname)s - %(message)s')

start_num = 27

def main():
    with open('solution/acm/list_journals.txt') as f:
        i = 0
        for l in f.readlines():
            i = i + 1
            if i >= start_num:
                process_link(l, i)
            


def process_link(link: str, i: int):
    logging.debug(link)
    try:
        redirect = requests.get(link)
        logging.debug(redirect.url)
        editors_url = redirect.url + '/editorial-board'
        editorial_board_content = requests.get(editors_url)
        with open(f"solution/acm/editorial_boards/{i}.html", "w") as f:
            f.write(editorial_board_content.text)
    except Exception as e:
        logging.error(f"{i} failed")
        logging.exception(e)




if __name__ == '__main__':
    logging.debug('Program start')
    main()
    logging.debug('Program finished')
