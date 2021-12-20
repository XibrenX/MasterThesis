{{ 
    config(materialized = 'table')
}}
SELECT 
    COALESCE(p.book_dblp_key, c.book_dblp_key) as book_dblp_key
  , SPLIT_PART(COALESCE(p.book_dblp_key, c.book_dblp_key), '/', 1) as book_dblp_key_type
  , SPLIT_PART(COALESCE(p.book_dblp_key, c.book_dblp_key), '/', 2) as book_dblp_key_conf
  , SPLIT_PART(COALESCE(p.book_dblp_key, c.book_dblp_key), '/', 3) as book_dblp_key_period
  , COALESCE(p.member_name, c.member_name) as member_name
  , MD5(COALESCE(p.member_name, c.member_name)) as member_name_anom
  , p.cnt_articles_written_in_book as cnt_published
  , p.mad as book_publishing_mad
  , p.ratio as publishing_ratio
  , c.cnt_author_cited_in_book as cnt_cited
  , c.mad_score as book_citations_mad
  , c.ratio as citation_ration
FROM {{ ref('analysis_lncs_members_publishing') }} p
FULL OUTER JOIN {{ ref('analysis_lncs_members_citations') }} c
ON p.book_dblp_key = c.book_dblp_key
AND p.member_name = c.member_name
WHERE p.cnt_articles_written_in_book IS NOT NULL
  AND c.cnt_author_cited_in_book IS NOT NULL
