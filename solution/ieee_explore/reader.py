import json
import os
import re
import datetime
import pathlib
import saver
import database

input_directory = "output"

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
schema_name = 'ieee_explore'

db = database.SqlServer(conn_str)
saver = saver.Saver(db)

def get_files():
    for filename in os.listdir(input_directory):
        if filename.endswith(".json"): 
            f = os.path.join(input_directory, filename)
            yield f



pattern = re.compile(r'(?<!^)(?=[A-Z])')
def to_camel_case(value):
    x = pattern.sub('_', value).lower()
    return x


cache_documents = []
cache_authors = []
cache_affiliations = []
cache_issn = []
cache_isbn = []

def write_to_database():
    print("flushing to db")
    global cache_documents
    global cache_authors
    global cache_affiliations
    global cache_issn
    global cache_isbn
    saver.save(schema_name, 'document', cache_documents)
    saver.save(schema_name, 'author', cache_authors)
    saver.save(schema_name, 'affiliation', cache_affiliations)
    saver.save(schema_name, 'issn', cache_issn)
    saver.save(schema_name, 'isbn', cache_isbn)
    cache_documents = []
    cache_authors = []
    cache_affiliations = []
    cache_issn = []
    cache_isbn = []


DOCUMENT_ATTRIBUTES = [
    "articleNumber",
    "endPage",
    "startPage",
    "issueLink",
    "publicationTitle",
    "displayPublicationTitle",
    "formulaStrippedArticleTitle",
    "publisher",
    "journalDisplayDateOfPublication",
    "volume",
    "issue",
    "conferenceDate",
    "displayDocTitle",
    "dateOfInsertion",
    "isJournal",
    "isBook",
    "isConference",
    "publicationDate",
    "isConference",
    "title",
    "confLoc",
    "content_type",
    "chronDate",
    "publicationDate",
    "publicationNumber",
    "articleId",
    "publicationTitle",
    "publicationYear",
    "subType",
    "doi",
    "doiLink",
    "issueLink",
    "xploreDocumentType",
    "_value",
    "lastupdate"
]
document_id = 0
def read_document(file):
    fname = pathlib.Path(file)
    valid_dts = datetime.datetime.fromtimestamp(fname.stat().st_ctime)
    # print(valid_dts)
    global document_id
    document_id = document_id + 1
    print(f"Processing: {file} (id: {document_id})")
    dblp_document_id = file.replace(f"{input_directory}\\", "").replace(".json", "")
    print(f"dblp_document_id: {dblp_document_id}")
    with open(file) as json_file:
        data = json.load(json_file)
        doc = {to_camel_case(a): str(data[a]) if a in data else None for a in DOCUMENT_ATTRIBUTES}
        doc["$_document_id"] = str(document_id)
        doc["$_extracted"] = str(valid_dts)
        doc["$_dblp_document_id"] = str(dblp_document_id)
        if "authors" in data:
            read_authors(data, document_id)
        if "isbn" in data:
            read_isbn(data, document_id)
        if "issn" in data:
            read_issn(data, document_id)
        # print(doc)
        cache_documents.append(doc)


ISBN_ATTRIBUTES = [
    "format",
    "value",
    "isbnType"
]
isbn_id = 0
def read_isbn(data, document_id):
    for isbn in data["isbn"]:
        global isbn_id
        isbn_id = isbn_id + 1
        row = {to_camel_case(a): str(isbn[a]) if a in isbn else None for a in ISBN_ATTRIBUTES}
        row["$_isbn_id"] = str(isbn_id)
        row["$_document_id"] = str(document_id)
        cache_isbn.append(row)

issn_id = 0
ISSN_ATTRIBUTES = [
    "format",
    "value"
]
def read_issn(data, document_id):
    for issn in data["issn"]:
        global issn_id
        issn_id = issn_id + 1
        row = {to_camel_case(a): str(issn[a]) if a in issn else None for a in ISSN_ATTRIBUTES}
        row["$_issn_id"] = str(issn_id)
        row["$_document_id"] = str(document_id)
        cache_issn.append(row)


AUTHOR_ATTRIBUTES = [
    "name",
    "firstName",
    "lastName",
    "id"
]
author_id = 0
affiliation_id = 0
def read_authors(data, document_id):
    for author in data["authors"]:
        global author_id
        author_id = author_id + 1
        # for a in AUTHOR_ATTRIBUTES:
        row = {to_camel_case(a): str(author[a]) if a in author else None for a in AUTHOR_ATTRIBUTES}
        row["$_author_id"] = str(author_id)
        row["$_document_id"] = str(document_id)
        # print(row)
        cache_authors.append(row)
        if "affiliation" in author:
            for affiliation in author["affiliation"]:
                global affiliation_id
                affiliation_id = affiliation_id + 1
                aff = {}
                aff["$_author_id"] = str(author_id)
                aff["$_affiliation_id"] = str(affiliation_id)
                aff["affiliation"] = str(affiliation)
                # print(aff)
                cache_affiliations.append(aff)


if __name__ == '__main__':
    print("program started")
    i = 0
    for f in get_files():
        i = i + 1
        read_document(f)
        if i % 1000 == 0:
            write_to_database()
    write_to_database()
    print("done")

