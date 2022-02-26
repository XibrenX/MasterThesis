{{ config(
    materialized = 'table',
    indexes = [ {'columns': ['person_hash_key'],
    'type': 'hash' },],
    tags = ["dblp"]
) }}

SELECT
    person_hash_key,
    'dblp_dump' AS "rec_src",
    orcid
FROM
    {{ ref('stg_person') }}
WHERE
    orcid IS NOT NULL
GROUP BY
    person_hash_key,
    orcid
