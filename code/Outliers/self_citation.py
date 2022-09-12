import numpy
import csv

import matplotlib.pyplot as plt

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

file = config['OUTLIER_DATA_DIR'] + '/export_self_citations.csv'

PERSON_NAME = 0
SELF_CITATION_COUNT = 1
CITATION_COUNT = 2

def select_cs(i):
    return i['cs']

def select_d(i):
    return i['d']

def main():
    persons = []

    with open(file, 'r', encoding='utf-8') as f:
        csv_reader = csv.reader(f)
        it = iter(csv_reader)
        next(it)
        for row in it:
            persons.append({
                'name': row[PERSON_NAME],
                'scs': int(row[SELF_CITATION_COUNT]),
                'cs': int(row[CITATION_COUNT])
            })

    scss = []

    for person in persons:
        scs = person['scs']
        while len(scss) <= scs:
            scss.append(0)
        scss[scs] += 1

    plt.figure()
    plt.bar(range(0, len(scss))[0:500], numpy.log(scss)[0:500])
    plt.xlabel('self citations')
    plt.ylabel('ln(occurence count)')
    plt.show()

    for person in persons:
        if person['cs'] == 0:
            person['d'] = 0
        else:
            person['d'] = person['scs'] / person['cs']

    persons = [p for p in persons if p['cs'] > 50]
    
    persons.sort(reverse=True, key=select_cs)
    persons.sort(reverse=True, key=select_d)

    ds = [p['d'] for p in persons]

    xs = [i for i in range(0, 101)]
    ys = [0 for _ in xs]

    for d in ds:
        if d != 0:
            ys[int(d * 100)] += 1

    fxs = [x for x in xs if ys[x] > 0]
    fys = [y for y in ys if y > 0]

    (m, c) = numpy.polyfit(fxs, numpy.log(fys), 1, w=fys)
    a = numpy.exp(c)
    b = numpy.exp(m)

    print('a:', a, 'b', b)

    curve = []
    for x in xs:
        curve.append(a * b**x)

    treshold = 0
    for x in xs:
        if curve[x] < 0.5:
            treshold = x
            print(f'Below treshold on {x}')
            break

    plt.figure()
    plt.bar(range(0, 101), ys)
    plt.xlabel('self citation percentage')
    plt.ylabel('occurence count')
    plt.plot(xs, curve, color='orange')
    plt.show()

    for i in range(0, len(persons)):
        person = persons[i]
        if person['d'] >= treshold / 100:
            print(f'{i}: {person}')

    print(len(persons))

if __name__ == '__main__':
    main()