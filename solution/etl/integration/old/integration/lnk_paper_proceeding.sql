{{ 
    config(materialized='table') 
}}
select 
  MD5(REPLACE(b."doi", 'https://doi.org/', '')) as "springer_lncs_book_surrogate_id"
, MD5(REPLACE(c."doi", 'https://doi.org/', '')) as "springer_lncs_paper_surrogate_id"
, MD5(REPLACE(c."doi", 'https://doi.org/', '')) as "paper_integration_id"
, MD5(REPLACE(b."doi", 'https://doi.org/', '')) as "proceeding_integration_id"
from {{ source('springer_lncs', 'book') }} b
left outer join {{ source('springer_lncs', 'chapter') }} c
on b."$_book_url" = c."$_book_url"