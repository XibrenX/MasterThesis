{{ 
    config(materialized='table') 
}}
SELECT
      MD5("name") as "member_id"
    , MD5("filename") as "book_id"
    , "role"
FROM {{ ref('clean_source' )}}
GROUP BY "name", "filename", "role"