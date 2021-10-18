import requests
import re
import xplore
import time
import pyodbc
import os

server = 'localhost'
db_name = 'study'
driver = '{ODBC Driver 17 for SQL Server}'
conn_str = f'Driver={driver};Server={server};Database={db_name};Trusted_Connection=yes;'
output_dir = "output"

def get_workload():
    print("getting workload")
    try:
        query = """
            SELECT [$_parent_object_id], [ee]
            FROM [study].[dblp_clean].[ee_clean]
            WHERE ee LIKE '%ieeexplore.ieee.org%'
            and [$_parent_object_type] IN ('article', 'incollection', 'inproceedings')
            and [$_parent_object_id] NOT IN (
                SELECT [$_parent_object_id] 
                FROM [dblp_clean].[doi] 
                GROUP BY [$_parent_object_id]
                )
            ORDER BY [$_parent_object_id]
        """
        conn = pyodbc.connect(conn_str)
        cursor = conn.cursor()
        cursor.execute(query)
        row = cursor.fetchone()
        res = []
        while row:
            res.append({
                "parent_object_id": str(row[0]),
                "ee": str(row[1])
                })
            row = cursor.fetchone()
        return res
    except:
        print("error occured")
    finally:
        cursor.close()

def download_content(workitem, session):
    url = workitem["ee"]
    id = workitem["parent_object_id"]
    filename = f"{output_dir}\\{id}.json"
    
    if os.path.isfile(filename):
        # print (f"File {filename} already exist: skipping")
        return False
    else:
        # try:
        x = requests.get(url)
        if x.status_code != 200:
            print(f"{workitem}: {x.status_code}")
            return False
        v = x.text
        y = re.findall("xplGlobal\.document\.metadata=.*;\s*<\/script>", v)
        try:
            t1 = y[0]
            t2 = re.sub(r';\s*<\/script>$', '', t1)
            t3 = re.sub(r'^xplGlobal\.document\.metadata=', '', t2)
            with open(filename, "w", encoding="utf-8") as f:
                f.write(t3)
        except:
            print(f"error occured: {workitem}")
        return True


def get_tor_session():
    session = requests.session()
    session.proxies = {}
    session.proxies['http'] = 'socks5://127.0.0.1:9150'
    session.proxies['https'] = 'socks5://127.0.0.1:9150'
    return session


if __name__ == '__main__':
    print("program started")
    # session = get_tor_session()
    session = None
    wl = get_workload()
    i = 0
    for wi in wl:
        i = i + 1
        if download_content(wi, session):
            time.sleep(1)
        if i % 100 == 0:
            print(f"Processed {i} items")
        
    print("done")

