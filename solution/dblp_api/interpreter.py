import requests
import logging

import codecs
from bs4 import BeautifulSoup
import time
import datetime
import re


def read_content(content) -> list:
    soup = BeautifulSoup(content, 'html.parser')
    conference_name = "< UNKOWN >"
    conference_id = "< UNKOWN >"
    events = []
    main_content = soup.find('div', id="main")
    headers = main_content.find_all('header')
    for h in headers:
        if 'id' in h.attrs and h["id"] == 'headline':
            conference_name = h.find('h1').text.strip()
            conference_id = h["data-bhtkey"]
        if h.find('h2') is not None:
            event_header = h.find('h2')
            event = {
                "conference_id": conference_id,
                "conference_name": conference_name,
                "event_id": event_header["id"],
                "event_name": event_header.text
            }
            events.append(event)
    return events


# test_file = 'software/dblp_scraper/example_docs/conference.html'
# html_doc = codecs.open(test_file, 'r')
# contents = html_doc.read()
# x = read_content(contents)
# print("done")

