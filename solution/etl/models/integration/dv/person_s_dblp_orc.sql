{{ 
    config(
        materialized='table',
        indexes=[      
            {'columns': ['hash_key'], 'type': 'hash'},        
            ]
        )
}}
SELECT 
      hash_key
    , 'dblp_dump' as "rec_src"
    , orcid
FROM {{ ref('stg_author') }}
WHERE orcid IS NOT NULL
GROUP BY hash_key, orcid