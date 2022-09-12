class LabelMapper:
    type_to_label = {
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

    label_to_type = {v: k for k, v in type_to_label.items()}

    def get_label(self, type):
        return self.type_to_label[type]

    def get_type(self, label):
        return self.label_to_type[label]

class Node:
    def __init__(self, id, label):
        self.id = id
        self.label = label

class Relationship:
    def __init__(self, id, label):
        self.id = id
        self.label = label

class Cycle:
    def __init__(self, nodes, relationships):
        self.nodes = nodes
        self.relationships = relationships
        self.label = ''.join([item.label for pair in zip(nodes, relationships) for item in pair])