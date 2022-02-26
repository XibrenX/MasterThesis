{{ 
    config(materialized='table') 
}}
WITH clean_author_cte AS (
   SELECT
          surrogate_id
        , orc
        , email
        , name
    FROM {{ ref('stg_lncs_author' ) }}
)
, source_author AS (
    SELECT 
          "$_paper_url"
        , doc_person_id
        , name
        , email
        , REGEXP_REPLACE("orc", 'https?://', '') as "orc"
    FROM {{ source('springer_lncs', 'author') }}
)
, orc_cte AS (
    SELECT 
          s."$_paper_url"
        , s.doc_person_id
        , c.surrogate_id as "author_surrogate_id"
    FROM source_author s
    LEFT OUTER JOIN 
    (
        SELECT "orc", surrogate_id
        FROM clean_author_cte
        GROUP BY "orc", surrogate_id
    ) c
    ON s.orc = c.orc
    WHERE s.orc IS NOT NULL
)
, email_cte as (
    SELECT 
          s."$_paper_url"
        , s.doc_person_id
        , c.surrogate_id as "author_surrogate_id"
    FROM source_author s
    LEFT OUTER JOIN 
    (
        SELECT "email", surrogate_id
        FROM clean_author_cte
        GROUP BY "email", surrogate_id
    ) c
    ON s.email = c.email
    WHERE s."orc" IS NULL and s.email IS NOT NULL
)
, name_cte AS (
    SELECT 
          s."$_paper_url"
        , s.doc_person_id
        , c.surrogate_id as "author_surrogate_id"
    FROM source_author s
    LEFT OUTER JOIN 
    (
        SELECT "name", surrogate_id
        FROM clean_author_cte
        GROUP BY "name", surrogate_id
    ) c
    ON s.name = c.name
    WHERE s."orc" IS NULL and s.email IS NULL
    ORDER BY s.name
)
, union_cte AS (
    SELECT *
    FROM orc_cte
    UNION
    SELECT *
    FROM email_cte
    UNION 
    SELECT *
    FROM name_cte
)
SELECT *
FROM union_cte

