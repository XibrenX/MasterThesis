import pyodbc
import arxiv
import database
import saver
import datetime
import time
import logging
import re

logging.basicConfig(level=logging.INFO, format='%(name)s - %(levelname)s - %(message)s')
server = 'localhost'
db_name = 'study'
driver = '{ODBC Driver 17 for SQL Server}'
conn_str = f'Driver={driver};Server={server};Database={db_name};Trusted_Connection=yes;'
rec_parallel = 1
DOCUMENT_FIELDS = (
    "entry_id",
    "updated",
    "published",
    "title",
    "journal_ref",
    "doi"
)
schema_name = 'arxiv'

db = database.SqlServer(conn_str)
saver = saver.Saver(db)


def fetch_keys():
    # print("Fetch keys from database")
    try:
        query = f"SELECT TOP {rec_parallel} [ee] FROM [dblp_clean].[ee_clean] WHERE [ee_type] = 'arxiv' "
        if db.table_exists(schema_name, "document"):
            query = f"""
            SELECT TOP {rec_parallel} d.[ee]
            FROM [dblp_clean].[ee_clean] d
            LEFT OUTER JOIN [arxiv].[document] a
            ON d.ee = a.[$_dblp_ee]
            WHERE d.[ee_type] = 'arxiv'
            AND a.[$_dblp_ee] IS NULL
            ORDER BY d.[ee]
            """

        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        cursor.execute(query)
        row = cursor.fetchone()
        res = []
        while row:
            res.append({"ee": str(row[0])})
            row = cursor.fetchone()
        return res
    except:
        print("error occured")
    finally:
        cursor.close()


def get_arxiv_id(ee) -> str:
    v = ee \
        .replace("https://arxiv.org/abs/", "") \
        .replace("http://arxiv.org/abs/", "") \
        .replace("http://arxiv.org/html/", "") \
        .replace("cs.pl", "cs.PL") # er zitten een paar gevallen in en de api is hoofdletter gevoelig
    return v


def get_arxiv_results(ids):
    if not isinstance(ids, list):
        raise TypeError('expect list as input')
    search = arxiv.Search(id_list=ids)
    return search.get()


def run_batch():
    documents = []
    authors = []
    print(f"Start running batch")
    r = fetch_keys()
    if len(r) == 0:
        return -1
    #DEBUG
    # r = [{'ee': 'http://arxiv.org/abs/0704.0002'}, {'ee': 'http://arxiv.org/abs/0704.0046'}, {'ee': 'http://arxiv.org/abs/0704.0050'}]
    for item in r:
        item["id"] = get_arxiv_id(item["ee"])
    ids = [item["id"] for item in r]
    timestamp = str(datetime.datetime.now())
    api_result = get_arxiv_results(ids)

    i = 0
    while True:
        try:
            x = next(api_result)
            doc = process_document(r[i]["ee"], x)
            doc["$_api_extract_dts"] = timestamp
            doc["$_api_status"] = "SUCCEEDED"
            documents.append(doc)
            for a in x.authors:
                auth = process_author(r[i]["ee"], x.entry_id, a)
                auth["$_api_extract_dts"] = timestamp
                authors.append(auth)
            i = i+1 
        except AttributeError as error:
            print(f"error has occured in retrieving values from api_result, ee: {r[i]}")
            print(error)
            err = { df: None for df in DOCUMENT_FIELDS }
            err["$_dblp_ee"] = r[i]["ee"]
            err["$_api_extract_dts"] = timestamp
            err["$_api_status"] = "FAILED"
            documents.append(err)
            i = i+1 
            break
        except arxiv.HTTPError as error:
            print(f"error has occured in retrieving values from api_result, ee: {r[i]}")
            print(error)
            err = { df: None for df in DOCUMENT_FIELDS }
            err["$_dblp_ee"] = r[i]["ee"]
            err["$_api_extract_dts"] = timestamp
            err["$_api_status"] = "FAILED"
            documents.append(err)
            i = i+1 
            break
        except StopIteration:
            print(f"iteration done")
            break

    saver.save(schema_name, 'document', documents)
    saver.save(schema_name, 'author', authors)

    print(f"Done running batch ({i} records)")
    return i


def process_document(ee, arxiv_result) -> dict:
    result = { df: str(arxiv_result.__getattribute__(df)) for df in DOCUMENT_FIELDS }
    result["$_dblp_ee"] = ee
    return result


def process_author(ee, entry_id, author) -> dict:
    return {
        "$_dblp_ee": ee,
        "$_arxiv_entry_id": entry_id,
        "name": author.name
    }

if __name__ == '__main__':
    print("Program started")
    while True:
        x = run_batch()
        if x == -1:
            break
        print("Sleeping...")
        time.sleep(4)
    print("Program finished")


