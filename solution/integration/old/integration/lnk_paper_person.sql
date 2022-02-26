{{ 
    config(materialized='table') 
}}
with source_cte as (
    select 
        MD5(REPLACE("doi", 'https://doi.org/', '')) as "springer_lncs_paper_surrogate_id"
        , author_surrogate_id as "springer_lncs_author_surrogate_id"
    from {{ ref('bus_author_chapter') }} bac
    left outer join {{ source('springer_lncs', 'chapter') }} c
    on bac."$_paper_url" = c."$_paper_url"
)
select ip.integration_id as "paper_integration_id", ipers.integration_id as "person_integration_id"
from source_cte s
left outer join {{ ref('paper') }} ip
on s."springer_lncs_paper_surrogate_id" = ip."springer_lncs_surrogate_id"
left outer join {{ ref('person') }} ipers
ON s.springer_lncs_author_surrogate_id = ipers."springer_lncs_surrogate_id"