import configparser
from os import path

def read_config(path) -> configparser.SectionProxy:
    with open(path, 'r') as f:
        config_string = '[SECTION]\n' + f.read()
    config = configparser.ConfigParser()
    config.read_string(config_string)
    return config['SECTION']
    
config = read_config(path.join(path.dirname(__file__), './config'))