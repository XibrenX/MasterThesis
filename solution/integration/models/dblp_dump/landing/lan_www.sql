{{ config(
    materialized = 'table',
    indexes = [ { 'columns': ["id"],
    'type': 'hash' }],
    tags = ["dblp"]
) }}

SELECT
    CAST(
        "$_object_id" AS INTEGER
    ) AS "id",
    CAST(
        mdate AS DATE
    ) AS "mdate",
    NULLIF(
        "key",
        ''
    ) AS "dblp_key",
    NULLIF(
        publtype,
        ''
    ) AS "publtype"
FROM
    {{ source(
        'dblp_dump',
        'www'
    ) }}
GROUP BY
    "$_object_id",
    mdate,
    "key",
    publtype
