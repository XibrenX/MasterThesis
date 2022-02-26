{{ config(
    materialized = 'table',
    indexes = [ { 'columns': ["article_hash_key"],
    'type': 'hash' }],
        tags=["dblp"]
) }}

SELECT
    'dblp' AS rec_src,
    article_hash_key,
    publtype,
    title,
    "year",
    journal,
    volume,
    pages,
    "number"
FROM
    {{ ref('stg_article') }}
