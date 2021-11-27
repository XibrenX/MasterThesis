{{ 
    config(materialized='table') 
}}
SELECT
      "name" as "member_name"
    , "filename" as "book_filename"
    , "role"
FROM {{ ref('clean_source' )}}
GROUP BY "name", "filename", "role"