import document_parser
import saver
import database
import logging
import os
import sys

server = 'localhost'
db_name = 'study'
driver = '{ODBC Driver 17 for SQL Server}'
conn_str = f'Driver={driver};Server={server};Database={db_name};Trusted_Connection=yes;'
schema_name = 'springer_lncs'

input_dir = '/media/nas/ewoud/thesis/lncs_front_matter/pdf2htmlex_out'


def run():
    db = database.SqlServer(conn_str)
    saver = saver.Saver(db)
    dp = DocumentParser()
    for filename in os.listdir(input_dir):
        if filename.endswith(".html"):
            doc = {"filename": filename}
            file_path = print(os.path.join(input_dir, filename))
            logging.info(f"Processing: {file_path}")
            try:
                content = dp.parse(file_path)
                for c in content:
                    c["filename"] = filename
                status = "SUCCEEDED"
            except: # catch *all* exceptions
                error = sys.exc_info()[0]
                doc["error_message"] = error
                status = "FAILED"
            doc["status"] = status
            saver.save(schema_name, "parser_document", [doc])
            saver.save(schema_name, "parser_members", content)

if __name__ == '__main__':
    run()