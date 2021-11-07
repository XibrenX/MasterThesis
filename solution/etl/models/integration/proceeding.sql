{{ 
    config(materialized='table') 
}}
SELECT 
      sl.surrogate_id as "integration_id"
      ,book_title
    ,book_subtitle
        ,doi
        ,ebook_isbn
        ,ebook_packages
        ,edition_number
        ,front_matter_url
        ,publisher_name
        ,"series_e-issn"
        ,series_issn
        ,series_title
        ,softcover_isbn
        ,hardcover_isbn
    , sl.surrogate_id as "springer_lncs_surrogate_id"
FROM {{ ref('bus_book') }} sl