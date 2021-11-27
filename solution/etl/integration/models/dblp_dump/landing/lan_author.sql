{{ 
    config(
        materialized='table',
        indexes=[      
                    {
                        'columns': ["parent_id", "id"], 
                        'type': 'btree',
                        'unique': True
                    } , {
                        'columns': ["author_name"], 
                        'type': 'hash'
                    }   
                ],
        tags=["dblp"]
        )
}}
SELECT 
CAST("$_object_id" as INTEGER) as id
,CAST("$_parent_object_id" as INTEGER) as parent_id
,"$_parent_object_type" as parent_type
,NULLIF(author, '') as author_name
,NULLIF(orcid, '') as orcid
,NULLIF(bibtex, '') as bibtex
,NULLIF(aux, '') as aux
FROM {{ source('dblp_dump', 'author') }}
GROUP BY "$_object_id", "$_parent_object_id", "$_parent_object_type", "orcid", "bibtex", "aux", "author"