{{ 
    config(materialized='table') 
}}
SELECT 
      COALESCE(sl.surrogate_id, lfm.surrogate_id) as "integration_id"
    , COALESCE(sl."name", lfm."name") as "name"
    , sl.surrogate_id as "springer_lncs_surrogate_id"
    , lfm.surrogate_id as "lncs_front_matter_surrogate_id"
FROM {{ ref('bus_author') }} sl
FULL OUTER JOIN {{ ref('bus_member') }} lfm
ON sl.name = lfm.name
GROUP BY sl.surrogate_id, lfm.surrogate_id, sl."name", lfm."name"