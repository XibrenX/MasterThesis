{{ config(
    materialized = 'table',
    indexes = [ 
        {'columns': ['person_hash_key'],'type': 'hash' },
        {'columns': ['name'],'type': 'hash' }
        ],
    tags = ["dblp"]
) }}

SELECT
    person_hash_key,
    'dblp_dump' AS "rec_src",
    "name"
FROM
    {{ ref('stg_person') }}
GROUP BY
    person_hash_key,
    "name"
