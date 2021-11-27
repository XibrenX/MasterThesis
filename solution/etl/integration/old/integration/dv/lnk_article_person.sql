{{ config(
    materialized = 'table',
    indexes = [ { 'columns': ["person_hash_key", "article_hash_key"],
    'type': 'btree',
    'unique': True }],
    tags = ["dblp"]
) }}

SELECT
    MD5(CONCAT(article_hash_key, person_hash_key)) AS lnk_hash_key,
    article_hash_key,
    person_hash_key
FROM
    {{ ref('stg_article_person') }}
GROUP BY
    article_hash_key,
    person_hash_key
