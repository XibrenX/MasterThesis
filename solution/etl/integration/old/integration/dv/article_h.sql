{{ config(
    materialized = 'table',
    indexes = [ { 'columns': ["article_hash_key"],
    'type': 'hash' }],
        tags=["dblp"]
) }}

SELECT
    article_hash_key,
    article_business_key
FROM
    {{ ref('stg_article') }}
GROUP BY
    article_hash_key,
    article_business_key
