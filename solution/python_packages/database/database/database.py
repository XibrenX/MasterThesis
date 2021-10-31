import pyodbc
import logging

class Generic:
    def execute_bool(self, query) -> bool:
        logging.debug(f"Executing query: {query}")
        conn = self.get_connection()
        with conn:
            cursor = conn.cursor()
            cursor.execute(query)
            return cursor.fetchone() is not None


    def execute_query(self, query):
        logging.debug(query)
        conn = self.get_connection()
        with conn:
            cursor = conn.cursor()
            cursor.execute(query)


    def execute_query_result(self, query) -> list:
        logging.debug(f"Exeuting query: {query}")
        conn = self.get_connection()
        with conn:
            cursor = conn.cursor()
            cursor.execute(query)
            row = cursor.fetchone()
            result = []
            columns = [column[0] for column in cursor.description]
            for row in cursor.fetchall():
                result.append(dict(zip(columns, row)))
            return result



    def execute_many(self, query, data):
        conn = self.get_connection()
        with conn:
            cursor = conn.cursor()
            cursor.executemany(query, data)


    def create_schema(self, schema_name):
        query = f"CREATE SCHEMA {schema_name}"
        self.execute_query(query)


    

    

class SqlServer(Generic):

    def __init__(self, connection_string):
        self.connection_string = connection_string


    def get_connection(self):
        return pyodbc.connect(self.connection_string)


    def schema_exists(self, schema_name) -> bool:
        query = f"SELECT * FROM sys.schemas WHERE name = '{schema_name}'"
        return self.execute_bool(query)


    def table_exists(self, schema_name, table_name) -> bool:
        query = f"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema_name}' AND TABLE_NAME = '{table_name}';"
        return self.execute_bool(query)
    

    def get_column_info(self, schema_name, table_name) -> dict:
        query = f"SELECT [COLUMN_NAME], [CHARACTER_MAXIMUM_LENGTH] FROM INFORMATION_SCHEMA.COLUMNS WHERE [TABLE_SCHEMA] = '{schema_name}' AND [TABLE_NAME] = '{table_name}'; "
        conn = self.get_connection()
        with conn:
            cursor = conn.cursor()
            cursor.execute(query)
            row = cursor.fetchone()
            res = {}
            while row:
                res[str(row[0])] = int(row[1])
                row = cursor.fetchone()
            return res

    
    def create_table(self, schema_name, table_name, attributes):
        query = f"CREATE TABLE [{schema_name}].[{table_name}] ("
        ap = [f"[{an}] NVARCHAR({attributes[an]}) NULL" for an in attributes]
        query += ",".join(ap)
        query += ");"
        self.execute_query(query)
        logging.debug("Done creating table")


    def change_size(self, schema_name, table_name, column_name, new_size):
        query = f"ALTER TABLE [{schema_name}].[{table_name}] ALTER COLUMN [{column_name}] NVARCHAR({new_size}) NULL "
        self.execute_query(query)


    def add_columns(self, schema_name, table_name, columns):
        query = f"ALTER TABLE [{schema_name}].[{table_name}] ADD "
        column_parts = [f"[{c}] NVARCHAR({columns[c]}) NULL " for c in columns]
        query += ", ".join(column_parts) + ";"
        self.execute_query(query)


    def insert_into(self, schema_name, table_name, data):
        db_data = [tuple(dic.values()) for dic in data]
        columns = ", ".join(["[" + x + "]" for x in data[0]])
        qm = ", ".join(["?"] * len(data[0]))
        query = f"INSERT INTO [{schema_name}].[{table_name}] ({columns}) VALUES ({qm})"
        self.execute_many(query, db_data)


class Postgress(Generic):
    def __init__(self, server, database, user, password):
        self.connection_string = (
            "DRIVER={PostgreSQL Unicode};"
            f"Server={server};"
            "Port=5432;"
            f"Database={database};"
            f"uid={user};"
            f"pwd={password};"
            "sslmode=require"
        )


    def get_connection(self):
        return pyodbc.connect(self.connection_string)


    def schema_exists(self, schema_name) -> bool:
        logging.info("Posgres Schema exists")
        query = f"SELECT schema_name FROM information_schema.schemata WHERE schema_name = '{schema_name}';"
        return self.execute_bool(query)


    def table_exists(self, schema_name, table_name) -> bool:
        query = f"""
        SELECT 1
        FROM   information_schema.tables 
        WHERE  table_schema = '{schema_name}'
        AND    table_name = '{table_name}'
        """
        return self.execute_bool(query)
    

    def get_column_info(self, schema_name, table_name) -> dict:
        query = f"""
        SELECT
            COLUMN_NAME, CHARACTER_OCTET_LENGTH
        FROM
            information_schema.columns
        WHERE
            table_schema = '{schema_name}'
            AND table_name = '{table_name}';
        """
        conn = self.get_connection()
        with conn:
            cursor = conn.cursor()
            cursor.execute(query)
            row = cursor.fetchone()
            res = {}
            while row:
                res[str(row[0])] = int(row[1])
                row = cursor.fetchone()
            return res

    
    def create_table(self, schema_name, table_name, attributes):
        query = f"CREATE TABLE \"{schema_name}\".\"{table_name}\" ("
        ap = [f"\"{an}\" TEXT NULL" for an in attributes]
        query += ",".join(ap)
        query += ");"
        self.execute_query(query)
        print("Done creating table")


    def change_size(self, schema_name, table_name, column_name, new_size):
        pass


    def add_columns(self, schema_name, table_name, columns):
        query = f"ALTER TABLE \"{schema_name}\".\"{table_name}\" "
        column_parts = [f"ADD COLUMN \"{c}\" TEXT NULL " for c in columns]
        query += ", ".join(column_parts) + ";"
        self.execute_query(query)

    
    def insert_into(self, schema_name, table_name, data):
        db_data = [tuple(dic.values()) for dic in data]
        columns = ", ".join(["\"" + x + "\"" for x in data[0]])
        qm = ", ".join(["?"] * len(data[0]))
        query = f"INSERT INTO \"{schema_name}\".\"{table_name}\" ({columns}) VALUES ({qm})"
        self.execute_many(query, db_data)
    