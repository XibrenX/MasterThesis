{{ 
    config(
        materialized='table',
        indexes=[      
                    {
                        'columns': ["id"], 
                        'type': 'hash'
                    }    
                ],
        tags=["dblp"]
        )
}}
SELECT CAST("$_object_id" as INTEGER) as id
,CAST(mdate as DATE) as mdate
,key as dblp_key
,NULLIF(publtype, '') as publtype
FROM {{ source('dblp_dump', 'article') }}