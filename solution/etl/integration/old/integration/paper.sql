{{ 
    config(materialized='table') 
}}
SELECT 
      sl.surrogate_id as "integration_id"
    , "title"
    , "doi"
    , sl.surrogate_id as "springer_lncs_surrogate_id"
FROM {{ ref('bus_chapter') }} sl
