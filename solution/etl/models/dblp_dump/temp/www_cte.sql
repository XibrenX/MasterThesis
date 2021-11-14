{{ config(
    materialized = 'table',
    schema = 'tempory_tables',
    indexes = [ { 'columns': ["author_name"],
    'type': 'hash' }],
    tags = ["dblp"]
) }}

WITH unique_author_cte AS (

    SELECT
        author_name,
        parent_id
    FROM
        {{ ref('lan_author') }}
    WHERE
        parent_type = 'www'
    GROUP BY
        parent_id,
        author_name
)
SELECT
    A.author_name,
    w.dblp_key AS person_business_key,
    MD5(
        w.dblp_key
    ) AS person_hash_key
FROM
    unique_author_cte A
    LEFT OUTER JOIN {{ ref('lan_www') }}
    w
    ON A.parent_id = w.id
