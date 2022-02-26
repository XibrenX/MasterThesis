{{ config(
    materialized = 'table',
    indexes = [ { 'columns': ["id"],
    'type': 'hash' }],
    tags = ["dblp", "landing"]
) }}

SELECT
    CAST(
        "$_object_id" AS INTEGER
    ) AS id,
    CAST(
        mdate AS DATE
    ) AS mdate,
    key AS dblp_key,
    NULLIF(
        publtype,
        ''
    ) AS publtype
FROM
    {{ source(
        'dblp_dump',
        'incollection'
    ) }}
