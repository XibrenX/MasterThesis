{{ 
    config(
        materialized='table',
        indexes=[      
            {'columns': ['hash_key'], 'type': 'hash'},        
            ]
        )
}}
SELECT 
      business_key
    , hash_key
FROM {{ ref('stg_author') }}
GROUP BY business_key, hash_key