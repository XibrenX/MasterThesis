{{ config(
    materialized = 'table',
    indexes = [ 
        {'columns': ['dblp_object_key'],'type': 'hash' },
        {'columns': ['dblp_unique_person_key'],'type': 'hash' }
     ],
    tags = ["acm"]
) }}
select affiliation as acm_affiliation
,country as acm_country
,journal as acm_journal
,name as acm_name
,role as acm_role
,dblp_person.dblp_unique_person_key
,dblp_article.*
from {{ source('acm', 'editorial_boards') }} acm_boards
left outer join {{ ref('stg_dblp_person') }} dblp_person
on acm_boards.name = dblp_person.dblp_name
left outer join {{ ref('stg_dblp_article_person') }} dblp_article_person
on dblp_person.dblp_unique_person_key = dblp_article_person.dblp_unique_person_key
left outer join {{ ref('stg_dblp_article') }} dblp_article
on dblp_article_person.dblp_object_key = dblp_article.dblp_object_key
