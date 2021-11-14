{{ config(
    materialized = 'table',
    indexes = [ { 'columns': ["article_hash_key"],
    'type': 'hash' }]
) }}

SELECT
    'dblp' AS rec_src,
    article_hash_key,
    doi_link,
    doi
FROM
    {{ ref('stg_article_doi') }}
GROUP BY
    article_hash_key,
    doi_link,
    doi
