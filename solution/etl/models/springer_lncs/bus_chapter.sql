{{ 
    config(materialized='table') 
}}
SELECT 
      MD5(REPLACE("doi", 'https://doi.org/', '')) as "surrogate_id"
    , doi
    , title
FROM {{ source('springer_lncs', 'chapter') }}