import pyodbc
import arxiv
import database
import saver
import datetime
import time
import logging
import requests

logging.basicConfig(level=logging.INFO, format='%(name)s - %(levelname)s - %(message)s')
server = 'localhost'
db_name = 'study'
driver = '{ODBC Driver 17 for SQL Server}'
conn_str = f'Driver={driver};Server={server};Database={db_name};Trusted_Connection=yes;'
schema_name = 'dl_acm'

db = database.SqlServer(conn_str)
saver = saver.Saver(db)


def get_urls():
    query = """
        SELECT [$_parent_object_id], [ee]
        FROM [study].[dblp_clean].[ee_clean]
        WHERE ee LIKE '%dl.acm.org%'
        and [$_parent_object_type] IN ('article', 'incollection', 'inproceedings')
        and [$_parent_object_id] NOT IN (SELECT [$_parent_object_id] FROM [dblp_clean].[doi] GROUP BY [$_parent_object_id])
    """
    try:
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        cursor.execute(query)
        row = cursor.fetchone()
        res = []
        while row:
            res.append({
                "$_dblp_document_id": str(row[0]),
                "ee": str(row[1]),
                })
            row = cursor.fetchone()
        return res
    except:
        print("error occured")
    finally:
        cursor.close()


def get_redirect_url(acm_url):
    x = requests.get(acm_url)
    redirect_url = x.url
    return redirect_url


if __name__ == '__main__':
    print("program started")
    content = []
    raw_urls = get_urls()
    i = 0
    for u in raw_urls:
        i = i + 1
        u["redirect_url"] = get_redirect_url(u["ee"])
        time.sleep(1)
        content.append(u)
        if i % 100 == 0:
            print(f"flushing ({i} rows processed)")
            saver.save(schema_name, 'url', content)
            content = []
    saver.save(schema_name, 'url', content)
    print(f"done (total processed: {i} records)")
