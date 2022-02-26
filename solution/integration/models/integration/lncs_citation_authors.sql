{{ config(
  materialized = 'table',
  indexes = [ 
        {'columns': ['from_dblp_key'],'type': 'hash' },
        {'columns': ['to_author_name'],'type': 'hash' } 
      ],
) }}
select 
    slc."$_dblp_key" as from_dblp_key
  , ocr.citing as from_doi
  , ocr.cited as to_doi
  , dblp_doi.dblp_object_key as to_dblp_key
  , dblp_person.dblp_unique_person_key as to_dblp_unique_person_key
  , dblp_person.dblp_name as to_author_name
from {{ source('springer_lncs', 'chapter') }} slc
left outer join {{ source('opencitations_dump', 'reference') }} ocr
on TRIM(REPLACE(slc.doi, 'https://doi.org/', '')) = TRIM(ocr.citing)
left outer join {{ ref('stg_dblp_doi') }} dblp_doi
on TRIM(ocr.cited) = dblp_doi.dblp_doi
left outer join {{ ref('stg_dblp_article_person') }} dblp_article_person
on dblp_doi.dblp_object_key = dblp_article_person.dblp_object_key
left outer join {{ ref('stg_dblp_person') }} dblp_person
on dblp_article_person.dblp_unique_person_key = dblp_person.dblp_unique_person_key


