{{ config(
  materialized = 'table',
  indexes = [ 
    { 'columns': ["lncs_paper_doi"], 'type': 'hash' }
  ],
  tags = ["lncs", "staging"]
) }}
with book_cte AS (
  select  
 "$_dblp_key"     as dblp_key
,"$_book_url"
,REPLACE(doi, 'https://doi.org/', '') as lncs_book_doi
,doi              as lncs_book_doi_link
,book_subtitle    as lncs_book_subtitle
,book_title       as lncs_book_title
,ebook_isbn       as lncs_book_ebook_isbn
,edition_number   as lncs_book_edition_number
,front_matter_url as lncs_book_front_matter_url
,publisher_name   as lncs_book_publisher_name
,"series_e-issn"  as lncs_book_series_e_issn
,series_issn      as lncs_book_series_issn
,series_title     as lncs_book_series_title
,softcover_isbn   as lncs_book_softcover_isbn
,series_abbreviated_title as lncs_book_series_abbreviated_title
,hardcover_isbn   as lncs_book_series_hardcover_isbn
from {{ source('springer_lncs', 'book') }}
)

SELECT
    b."$_book_url" as lncs_book_url
  , c."$_paper_url" as lncs_chapter_url
  , REPLACE(c.doi, 'https://doi.org/', '') as lncs_paper_doi
  , c.doi as lncs_chapter_doi_link
  , c.online_isbn as lncs_chapter_online_isbn
  , c.print_isbn as lncs_chapter_print_isbn
  , c.publisher_name as lncs_chapter_publisher_name
  , c.title as lncs_chapter_title
  , c.first_online as lncs_chapter_first_online
  , c.revised as lncs_chapter_revised
  , b.*
FROM {{ source('springer_lncs', 'chapter') }} c
LEFT OUTER JOIN {{ source('springer_lncs', 'book') }} b
  ON c."$_book_url" = b."$_book_url"

