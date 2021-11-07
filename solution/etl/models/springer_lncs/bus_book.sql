select 
MD5(REPLACE("doi", 'https://doi.org/', '')) as "surrogate_id"
,"$_book_url"
,"$_dblp_key"
,"$_extract_dts"
,book_subtitle
,book_title
,copyright_information
,doi
,ebook_isbn
,ebook_packages
,edition_number
,front_matter_url
,number_of_illustrations
,number_of_pages
,publisher_name
,"series_e-issn"
,series_issn
,series_title
,softcover_isbn
,series_abbreviated_title
,license
,authors
,hardcover_isbn
from {{ source('springer_lncs', 'book') }}