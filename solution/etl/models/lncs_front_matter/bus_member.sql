{{ 
    config(materialized='table') 
}}
SELECT
      MD5("name") as "surrogate_id"
    , "name"
FROM {{ ref('clean_source' )}}
GROUP BY "name"