{{ 
    config(
        materialized='table',
        indexes=[      
            {'columns': ['person_hash_key'], 'type': 'hash'},        
            ]
        )
}}
SELECT 
      person_business_key
    , person_hash_key
FROM {{ ref('stg_person') }}
GROUP BY business_key, hash_key