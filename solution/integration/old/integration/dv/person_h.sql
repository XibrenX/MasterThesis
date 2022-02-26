{{ config(
    materialized = 'table',
    indexes = [ {'columns': ['person_hash_key'],
    'type': 'hash' },],
    tags = ["dblp"]
) }}

SELECT
    person_business_key,
    person_hash_key
FROM
    {{ ref('stg_person') }}
GROUP BY
    person_business_key,
    person_hash_key
