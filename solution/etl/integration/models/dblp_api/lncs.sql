WITH dblp_cte AS
(
    SELECT 
        info_key as dblp_key,
        info_ee as "url"
    FROM 
        {{ source('dblp_api', 'document') }}
    WHERE 
        (
            info_venue = 'Lecture Notes in Computer Science' 
            OR info_venue_0 = 'Lecture Notes in Computer Science' 
            OR info_venue_1 = 'Lecture Notes in Computer Science'
        )
        AND info_ee IS NOT NULL
    GROUP BY info_ee, info_key
)
, man_cte AS
(
    SELECT
        dblp_key,
        "url"
    FROM 
        {{ ref('manual_dblp_api_document') }}
    WHERE 
        "type" = 'LNCS'
)

SELECT dblp_key
     , "url"
FROM dblp_cte

UNION

SELECT dblp_key
     , "url"
FROM man_cte