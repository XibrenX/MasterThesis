import csv
import logging
from ntpath import join
import re
from pathlib import Path
import os
import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

def rec_replace(pattern: str, value: str, replacement: str) -> str:
    while True:
        nv = value.replace(pattern, replacement)
        if nv == value:
            return nv
        value = nv

def print_latex_leader():
    print(';'.join(['pattern', 'count', 'rectotal', 'recmed', 'recp75', 'recmax', 'nortotal', 'normed', 'norp75', 'normax']))

def print_latex(pattern, rec, nor):
    count = len(nor)
    total_nor = sum(nor)
    total_rec = sum(rec)

    med = int(count / 2)
    p75 = int(count * 3 / 4)

    print(';'.join(map(str, [f'\\cycle{{{pattern}}}', "{:,}".format(count), "{:,}".format(total_rec), "{:,}".format(rec[med]), "{:,}".format(rec[p75]), "{:,}".format(rec[count -1]), "{:,}".format(total_nor), "{:,}".format(nor[med]), "{:,}".format(nor[p75]), "{:,}".format(nor[count -1])])))

def print_excel_leader():
    print('\t'.join(['pattern', 'count', 'total_rec', 'rec[med]', 'rec[p75]', 'rec[count -1]', 'total_nor', 'nor[med]', 'nor[p75]', 'nor[count -1]']))

def print_excel(pattern, rec, nor):
    count = len(nor)
    total_nor = sum(nor)
    total_rec = sum(rec)

    med = int(count / 2)
    p75 = int(count * 3 / 4)

    print('\t'.join(map(str, [pattern, count, total_rec, rec[med], rec[p75], rec[count -1], total_nor, nor[med], nor[p75], nor[count -1]])))

def print_csv_leader():
    print(','.join(['pattern', 'count', 'total_rec', 'rec_med', 'rec_p75', 'rec_max', 'total_nor', 'nor_med', 'nor_p75', 'nor_max']))

def print_csv(pattern, rec, nor):
    count = len(nor)
    total_nor = sum(nor)
    total_rec = sum(rec)

    med = int(count / 2)
    p75 = int(count * 3 / 4)

    print(','.join(map(str, [pattern, count, total_rec, rec[med], rec[p75], rec[count -1], total_nor, nor[med], nor[p75], nor[count -1]])))

def sort_func(i):
    (pattern, rec, nor) = i
    return len(nor)

def format_pattern(pattern):
    # pattern = rec_replace('PaAaP', pattern, 'PaP')
    # pattern = rec_replace('PeReP', pattern, 'PeP')
    # pattern = rec_replace('PeJeP', pattern, 'PeP')
    # pattern = rec_replace('PpRpP', pattern, 'PpP')
    # pattern = rec_replace('RfCfR', pattern, 'RCR')
    # pattern = rec_replace('AbIbA', pattern, 'AIA')
    # pattern = rec_replace('AbRbA', pattern, 'ARA')
    # pattern = rec_replace('AbIfVfIbA', pattern, 'AIVIA')
    # pattern = rec_replace('IfVfI', pattern, 'IVI')

    # ms = re.findall('A(?:c͔A){2,}', pattern)
    # ms.sort(reverse=True)
    # for m in ms:
    #     pattern = pattern.replace(m, f'Ac͔{int(len(m) / 3)}A')
    # ms = re.findall('A(?:c͕A){2,}', pattern)
    # ms.sort(reverse=True)
    # for m in ms:
    #     pattern = pattern.replace(m, f'Ac͕{int(len(m) / 3)}A')
    # ms = re.findall('(([A-Z])([a-z])\\2(?:\\3\\2)+)', pattern)
    # for m in ms:
    #     pattern = pattern.replace(m[0], f'{m[1]}{m[2]}{int(len(m[0]) / 2)}{m[1]}')
    return pattern

def main():

    dir = config['CYCLE_STATS']

    p_count = 0

    persons_file = os.path.join(dir, 'persons_done.txt')

    with open(persons_file, 'r', encoding='utf-8') as f:
        for line in f:
            if line != "\n":
                p_count += 1

    files = Path(dir).glob('*.csv')

    all = []

    for file in files:
        pattern = Path(file).stem
        pattern = format_pattern(pattern)

        pattern = re.sub('c͔', '>c>', pattern)
        pattern = re.sub('c͕', '<c<', pattern)

        rec = []
        nor = []

        with open(file, 'r', encoding='utf-8') as f:
            reader = csv.reader(f)
            for row in reader:
                r_rec = int(row[1])
                r_nor = int(row[2])
                rec.append(r_rec)
                nor.append(r_nor)

        rec.sort()
        nor.sort()

        all.append((pattern, rec, nor))

    #print(f'Total persons {p_count}')
    print_latex_leader()

    all.sort(reverse=True, key=sort_func)

    for (pattern, rec, nor) in all:
        print_latex(pattern, rec, nor)
        # print_excel(pattern, rec, nor)
        # print_csv(pattern, rec, nor)

if __name__ == '__main__':
    main()