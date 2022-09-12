This folder contains several python scripts to scrape and process: ACM journals, editorial boards, issue pages, and front matters.

First the `journals.csv` is build by `parse_journals_overview_page.py`.

Next we used the code of @woutw to download their editorial board pages in `download_editorial_board_pages.py`. And parsed these with `parse_editorial_board_pages.py`


We scraped the issue pages of ACM with `download_issue_pages.py`.
These have been parsed with `parse_issue_page.py`

We then downloaded the issue pages front matters with `download_front_matter.py`.

We selected issue pages not older than a certain date in `select_issue_pages.py` to merge them with `merge_issues_with_editorial_board.py` and import them in Neo4j with `editorial_boards_with_issues.cql`