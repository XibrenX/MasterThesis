{{ 
    config(materialized='table') 
}}

WITH core_data AS (
    SELECT 
          a.name
        , COALESCE(m.email, a.email) as "email"
        , REGEXP_REPLACE(COALESCE(m.orc, a.orc), 'https?://', '') as "orc"
    FROM {{ source('springer_lncs', 'author') }} a
    LEFT OUTER JOIN {{ ref('manual_author') }} m
    ON LOWER(unaccent(a.name)) = LOWER(unaccent(m.name))
)
, orc_cte AS (
    SELECT
          MD5("orc") as "surrogate_id"
        , "orc"
        , "email"
        , "name"
        
    FROM
        core_data
    WHERE "orc" IS NOT NULL
    GROUP BY "orc"
        , "email"
        , "name"
)
-- emails without an orc
, email_cte AS (
    SELECT
          "email"
        , "name"
        , MD5("email") as "surrogate_id"
    FROM core_data
    WHERE "orc" IS NULL AND "email" IS NOT NULL
    GROUP BY "email"
        , "name"
)
, email_orc_cte AS (
    SELECT
          COALESCE(o.surrogate_id, e.surrogate_id) as "surrogate_id"
        , o.orc
        , e.email
        , COALESCE(o.name, e."name") as "name"
    FROM email_cte e
    FULL OUTER JOIN orc_cte o
    ON LOWER(o.email) = LOWER(e.email)

)


, only_name_cte as (
    SELECT MD5(name) as "surrogate_id"
        , "orc"
        , email
        , name
    FROM core_data
    WHERE orc IS NULL and email IS NULL
    GROUP BY name, email, orc
)
, name_email_orc_cte as (
    SELECT 
          COALESCE(oe.surrogate_id, n.surrogate_id) as "surrogate_id"
        , oe.orc as "orc"
        , oe.email as "email"
        , n.name as "name"
    FROM only_name_cte n
    LEFT OUTER JOIN email_orc_cte oe
    ON n.name = oe.name
)
, union_cte AS (
    SELECT *
    FROM orc_cte
    UNION
    SELECT *
    FROM email_orc_cte
    UNION
    SELECT *
    FROM name_email_orc_cte
)
SELECT *
FROM union_cte