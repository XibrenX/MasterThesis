import database

class Saver:

    def __init__(self, db):
        self.db = db


    def check_schema(self, schema_name):
        if not self.db.schema_exists(schema_name):
            print("Schema does not exists -> creating")
            self.db.create_schema(schema_name)


    def save(self, schema_name, table_name, data):
        if len(data) == 0:
            return
        al = {}
        for row in data:
            for an in row:
                length = 1
                if row[an] is not None:
                    length = len(row[an]) if len(row[an]) > 0 else 1
                if an not in al or al[an] <= length:
                    al[an] = length
        # print(al)

        if self.db.table_exists(schema_name, table_name):
            ec = self.db.get_column_info(schema_name, table_name)
            not_existent = {}
            to_adjust = {}
            for new_column in al:
                if new_column not in ec:
                    not_existent[new_column] = al[new_column]
                elif al[new_column] > ec[new_column]:
                    to_adjust[new_column] = al[new_column]
            
            if len(not_existent) > 0:
                print(f"adding {len(not_existent)} columns")
                self.db.add_columns(schema_name, table_name, not_existent)

            for c in to_adjust:
                print(f"adjusting column {c}")
                self.db.change_size(schema_name=schema_name, table_name=table_name, column_name=c, new_size=to_adjust[c])

        else:
            print("table does not exist")
            self.check_schema(schema_name)
            self.db.create_table(schema_name, table_name, al)

        # write the data    

        self.db.insert_into(schema_name, table_name, data)

        


