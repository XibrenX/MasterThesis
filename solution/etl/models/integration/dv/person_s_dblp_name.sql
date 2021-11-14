{{ 
    config(
        materialized='table',
        indexes=[      
            {'columns': ['person_hash_key'], 'type': 'hash'},        
            ]
        )
}}
SELECT 
      person_hash_key
    , 'dblp_dump' as "rec_src"
    , "name"    
FROM {{ ref('stg_person') }}
GROUP BY hash_key, "name"