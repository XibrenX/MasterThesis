{{ 
    config(materialized='table') 
}}
WITH www_cte AS
(
    SELECT a.author as "name", w.key as "business_key", MD5(w.key) as "hash_key"
    FROM {{ source('dblp_dump', 'author') }}  a
    LEFT OUTER JOIN {{ source('dblp_dump', 'www') }} w
    ON a."$_parent_object_id" = w."$_object_id"
    WHERE a."$_parent_object_type" = 'www'
)

SELECT 
      a.author as "name"
    , NULLIF(a.orcid, '') as "orcid"
    , COALESCE(w.business_key, a.author) as "business_key"
    , COALESCE(w.hash_key, MD5(a.author)) as "hash_key"
FROM {{ source('dblp_dump', 'author') }}  a
LEFT OUTER JOIN www_cte w
ON a.author = w.name
WHERE a."$_parent_object_type" <> 'www'

UNION 

SELECT 
      "name"
    , null as "orcid"
    , business_key
    , hash_key
FROM www_cte