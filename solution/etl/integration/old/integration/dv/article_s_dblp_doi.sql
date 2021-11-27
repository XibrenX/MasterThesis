{{ config(
    materialized = 'table',
    indexes = [ 
        { 'columns': ["article_hash_key"], 'type': 'hash' },
        { 'columns': ["doi"], 'type': 'hash' } ],
    tags=["dblp"]
) }}

SELECT
    'dblp' AS rec_src,
    article_hash_key,
    doi_link,
    doi_type,
    doi
FROM
    {{ ref('stg_article_doi') }}
GROUP BY
    article_hash_key,
    doi_link,
    doi_type,
    doi
