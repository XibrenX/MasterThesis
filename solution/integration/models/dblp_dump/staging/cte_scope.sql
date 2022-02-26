{{ config(
  materialized = 'table',
  indexes = [ 
    { 'columns': ["id"], 'type': 'hash' }
  ],
  tags = ["dblp"]
) }}

SELECT
    id
  , 'article' AS object_type
  , dblp_key
  , publtype
FROM
  {{ ref('lan_article') }}

UNION ALL

SELECT
    id
  , 'inproceedings' AS object_type
  , dblp_key
  , publtype
FROM
  {{ ref('lan_inproceedings') }}

UNION ALL

SELECT
    id
  , 'incollection' AS object_type
  , dblp_key
  , publtype
FROM
  {{ ref('lan_incollection') }}
