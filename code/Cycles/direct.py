import logging
import sys
sys.path.append('../') #Allow importing from base directory
from termcolor import colored

from read_config import config
from neo4j import GraphDatabase

def main():
    name = input("Please provide username")

    enable_log(logging.INFO, sys.stdout)

    driver = GraphDatabase.driver(config['NEO4J_SERVER'], auth=(config['NEO4J_USERNAME'], config['NEO4J_PASSWORD']))
    with driver.session() as session:
        persons = session.read_transaction(get_persons, name)
        if len(persons) == 0:
            print(f'Person {name} not found in dataset')
        else:
            if len(persons) > 1:
                realfound = False
                print(f'Multiple {name}s found in dataset:')
                for person in persons:
                    print(person['a.dblpName'])
                    realfound |= person['a.dblpName'] == name
                print()

                if not realfound:
                    return

            result = session.read_transaction(detect_cycles, name)
            print("Start processing")
            for record in result:
                proc_record(record)
        
    driver.close()

    out_mirror_cycles_types()
    print_cycles()


labels_map = {
    'Person': 'P',
    'DOI': 'D',
    'Article': 'A',
    'Issue': 'I',
    'Journal': 'J',
    'Orcid': 'O',
    'Proceeding': 'R',
    'Volume': 'V',

    'author_of': 'a',
    'belongs_to': 'b',
    'editor_of': 'e',
    'doi_of': 'd',
    'of': 'f',
    'orcid_of': 'o',
    'cites': 'c'
}

def incr(dict, key):
    dict[key] = dict[key] + 1 if key in dict else 1

cycles_count = {}
cycles_types = {}

def proc_record(record):
    path = record['p']
    incr(cycles_count, len(path.relationships))
    last_id = path.start_node.id
    line = labels_map[list(path.start_node.labels)[0]]
    for segement in path.relationships:
        line += labels_map[segement.type]
        next_node = segement.end_node if segement.end_node.id != last_id else segement.start_node
        line += labels_map[list(next_node.labels)[0]]
        last_id = next_node.id
    incr(cycles_types, line) 

def get_persons(tx, person_name):
    return list(tx.run("MATCH (a:Person) WHERE a.dblpName STARTS WITH $person_name RETURN a.dblpName", person_name=person_name))

def detect_cycles(tx, person_name):
    result = tx.run("MATCH p=(a:Person {dblpName: $person_name})<-[*2..5]->(a) RETURN p", person_name=person_name)
    records = list(result)
    return records

def get_all_relations(tx, node_id):
    result = tx.run("MATCH (n)-[r]-() WHERE ID(n) = $id RETURN type(r), (startNode(r) = n), count(*) as count", id=node_id)

def enable_log(level, output_stream):
    handler = logging.StreamHandler(output_stream)
    handler.setLevel(level)
    logging.getLogger("neo4j").addHandler(handler)
    logging.getLogger("neo4j").setLevel(level)

def out_mirror_cycles_types():
    global cycles_types
    new_dict = {}
    for key, value in cycles_types.items():
        key_mir = key[::-1]
        if key_mir == key:
            new_dict[key] = value / 2
        elif key_mir in new_dict:
            continue
        else:
            new_dict[key] = value
    cycles_types = new_dict

def print_cycles():
    for key, value in cycles_count.items():
        print(f'{key}: {value}')

    print()
    print('--')
    print()

    for key, value in cycles_types.items():
        print(f'{key}: {value}')

    print()
    print('--')
    print()

    labels_map = {
        'P': '(Person)',
        'D': '(DOI)',
        'A': '(Article)',
        'I': '(Issue)',
        'J': '(Journal)',
        'O': '(Orcid)',
        'R': '(Proceeding)',
        'V': '(Volume)',

        'a': '[author_of]',
        'b': '[belongs_to]',
        'e': '[editor_of]',
        'd': '[ee_of]',
        'f': '[of]',
        'o': '[orcid_of]',
        'c': '[cites]',
    }

    for key, value in cycles_types.items():
        path = key.translate(str.maketrans(labels_map))
        path = path.replace('author_of](Article)[author_of', colored('co-author_of', 'blue', attrs=['underline']))
        path = path.replace('belongs_to](Proceeding)[belongs_to', colored('same_proceeding', 'yellow', attrs=['underline']))
        path = path.replace('belongs_to](Issue)[belongs_to', colored('same_issue', 'yellow', attrs=['underline']))
        path = path.replace('belongs_to](Proceeding)[editor_of', colored('could-be-edited-by', 'red', attrs=['underline']))
        path = path.replace('editor_of](Proceeding)[belongs_to', colored('could-be-edited-by', 'red', attrs=['underline']))
        path = path.replace('editor_of](Proceeding)[editor_of', colored('co-editor_of', 'blue', attrs=['underline']))
        path = path.replace('author_of', colored('author_of', 'blue'))
        path = path.replace('belongs_to', colored('belongs_to', 'yellow'))
        path = path.replace('editor_of', colored('editor_of', 'red'))
        print(f'{path}: {value}')

if __name__ == '__main__':
    main()

