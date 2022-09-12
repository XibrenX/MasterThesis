import numpy
import csv

import matplotlib.pyplot as plt

import sys
sys.path.append('../') #Allow importing from base directory

from read_config import config

file = config['OUTLIER_DATA_DIR'] + '/export_pc_member_citations.csv'

CONFERENCE_CODE = 0
PROCEEDING_KEY = 1
PERSON_NAME = 3
COUNT = 4

def main():
    conferences = {}

    with open(file, 'r', encoding='utf-8') as f:
        csv_reader = csv.reader(f)
        it = iter(csv_reader)
        next(it)
        for row in it:
            conferences.setdefault(row[CONFERENCE_CODE], []).append({
                'proceeding': row[PROCEEDING_KEY], 
                'person': row[PERSON_NAME], 
                'count': int(row[COUNT])
            })

    show_conf_total(conferences)

    spire = conferences['spire']
    
    print('Exp regression')
    exp_regres_out(spire, True, 'spire')

    print('all')
    all_outliers(conferences)

def show_conf_total(conferences):
    counts = []

    for _, conf in conferences.items():
        for d in conf:
            add_count(counts, d['count'])
    
    xs = range(len(counts))

    plt.figure()
    plt.bar(xs[:16], counts[:16])
    plt.xlabel('number of citations of a PC member per proceeding')
    plt.ylabel('occurence count')

    plt.figure()
    plt.bar(xs[:16], numpy.log(counts[:16]))
    plt.xlabel('number of citations of a PC member per proceeding')
    plt.ylabel('ln(occurence count)')
    plt.show()

def get_score(d, c):
    return c

def exp_regres_out(conf, show = False, name = ''):
    counts = []

    for d in conf:
        add_count(counts, d['count'])

    xs = range(len(counts))
      
    fxs = [x for x in range(len(counts)) if counts[x] > 0]
    fys = [c for c in counts if c > 0]

    if len(fxs) <= 2:
        return {}

    (m, c) = numpy.polyfit(fxs, numpy.log(fys), 1, w=fys)
    a = numpy.exp(c)
    b = numpy.exp(m)

    if show:
        print('a:', a, 'b', b)

    curve = []
    for x in xs:
        curve.append(a * b**x)

    if show:
        for x in xs:
            if curve[x] < 0.5:
                print(f'Below treshold on {x}')
                break

        plt.bar(xs, counts)
        plt.plot(xs, curve, color='orange')
        plt.xlabel(f'number of citations of a PC member per proceeding in {name}')
        plt.ylabel('occurence count')

        plt.figure()
        plt.bar(xs, numpy.log(counts))
        plt.plot(xs, numpy.log(curve), color='orange')
        plt.xlabel(f'number of citations of a PC member per proceeding in {name}')
        plt.ylabel('ln(occurence count)')

        plt.show()

    ps_counts = {}
    for d in conf:
        add_count(ps_counts.setdefault(d['person'], []), d['count'])

    outliers = {}

    for person, p_counts in ps_counts.items():
        for x, y in enumerate(p_counts):
            d = y - curve[x]
            if d >= 0.5:
                score = get_score(d, x)
                outliers.setdefault(person, []).append(score)
                if show:
                    print(person, 'on', x, d, score)

    return outliers

def take_second(elem):
    return elem[1]

def all_outliers(conferences):
    outliers = {}

    for conf_name, conf in conferences.items():
        o = exp_regres_out(conf)
        for p, l in o.items():
            outliers[p] = outliers.setdefault(p, []) + l

    sum_outliers = sorted([(p, numpy.sum(l)) for (p, l) in outliers.items()], reverse=True, key=take_second)
    print(len(sum_outliers))
    for i in range(20):
        print(i, sum_outliers[i][0], sum_outliers[i][1])

def add_count(counts, count: int):
    while len(counts) <= count:
        counts.append(0)
    counts[count] += 1

if __name__ == '__main__':
    main()