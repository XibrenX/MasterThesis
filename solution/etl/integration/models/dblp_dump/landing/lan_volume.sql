{{ config(
    materialized = 'table',
    indexes = [ { 'columns': ["parent_id", "id"],
    'type': 'btree',
    'unique': True }],
    tags = ["dblp"]
) }}

SELECT
    CAST("$_object_id" AS INTEGER) AS id
  , CAST("$_parent_object_id" AS INTEGER) AS parent_id
  , "$_parent_object_type" AS parent_type
  , NULLIF(volume, '') as volume
FROM
    {{ source(
        'dblp_dump',
        'volume'
    ) }}
