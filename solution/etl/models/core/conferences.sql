WITH core_cte AS
(
    SELECT 
        "title"
        ,"acronym"
        ,"ranking"
        ,"dblp_url"
    FROM {{ ref('cln_conf_ranks') }}
)
, seed_cte AS
(
    SELECT
        "title"
        ,"acronym"
        ,REPLACE("dblp_url", '/index.html', '') as dblp_url
    FROM {{ ref('manual_dblp_url') }}
)
, join_cte AS
(
    SELECT 
        c."title",
        c."acronym",
        c."ranking",
        COALESCE(c."dblp_url", s."dblp_url") as dblp_url
    FROM 
        core_cte c
    LEFT OUTER JOIN seed_cte s
        ON c."title" = s."title"
        AND c."acronym" = s."acronym"
)
SELECT
    "title",
    "acronym",
    "ranking",
    REPLACE("dblp_url", 'https://dblp.uni-trier.de/db/', '') as dblp_key
FROM
    join_cte