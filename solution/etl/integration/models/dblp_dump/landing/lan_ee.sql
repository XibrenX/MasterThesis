{{ 
    config(
        materialized='table',
        indexes=[      
                    {
                        'columns': ["parent_id", "id"], 
                        'type': 'btree',
                        'unique': True
                    }    
                ],
        tags=["dblp"]
        )
}}
SELECT 
    CAST("$_object_id" as INTEGER) as id
  , CAST("$_parent_object_id" as INTEGER) as parent_id
  , "$_parent_object_type" as parent_type
  , NULLIF(ee, '') as doi_link
  , REPLACE(ee, 'https://doi.org/', '') as doi
FROM {{ source('dblp_dump', 'ee') }}
WHERE ee LIKE 'https://doi.org%'
