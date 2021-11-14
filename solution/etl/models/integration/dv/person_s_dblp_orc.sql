{{ 
    config(
        materialized='table',
        indexes=[      
                {'columns': ['author_hash_key'], 'type': 'hash'},        
            ]
        )
}}
SELECT 
      author_hash_key
    , 'dblp_dump' as "rec_src"
    , orcid
FROM {{ ref('stg_person') }}
WHERE orcid IS NOT NULL
GROUP BY hash_key, orcid