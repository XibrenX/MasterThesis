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
select CAST("$_object_id" as INTEGER) as id
,CAST("$_parent_object_id" as INTEGER) as parent_id
,"$_parent_object_type" as parent_type
,NULLIF(journal, '') as journal
from {{ source('dblp_dump', 'journal' ) }}