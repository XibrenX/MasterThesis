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
    , "name"    
FROM {{ ref('stg_author') }}
GROUP BY hash_key, "name"