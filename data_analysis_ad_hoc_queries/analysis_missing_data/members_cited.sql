
WITH members_cte AS (
    SELECT 
        REPLACE(REPLACE(book_filename, '_', '/'), '.pdf', '') as dblp_key
      , member_name
    FROM 
        lncs_front_matter.stg_lncs_fm_member_book
    GROUP BY
        book_filename
      , member_name
)
, book_author_agg AS (
    SELECT
        from_dblp_key
      , c.to_author_name
      , COUNT(*) as cnt_author_cited_in_book
    FROM
        integration.lncs_citation_authors c
    GROUP BY
        from_dblp_key
      , c.to_author_name
)
, book_member_cited_cte AS (
    SELECT 
      c.from_dblp_key as dblp_key
    , m.member_name
    , COALESCE(cnt_author_cited_in_book, 0) as cnt_author_cited_in_book
    FROM members_cte m
    LEFT OUTER JOIN book_author_agg c
    ON m.dblp_key = c.from_dblp_key
    AND m.member_name = c.to_author_name
)
, book_metrics_cte AS (
    SELECT
        dblp_key
      , PERCENTILE_CONT(0.5) WITHIN GROUP(ORDER BY cnt_author_cited_in_book) as median
    FROM
        book_member_cited_cte
    GROUP BY
        dblp_key
)
, book_metrics_mad_cte AS (
    SELECT 
        bm.dblp_key
      , PERCENTILE_CONT(0.5) WITHIN GROUP(ORDER BY ABS(bm.median - bmc.cnt_author_cited_in_book)) as mad
    FROM book_member_cited_cte bmc
    left outer JOIN book_metrics_cte bm
    on bmc.dblp_key = bm.dblp_key   
    GROUP BY bm.dblp_key
)
SELECT bm.*
    , bmc.member_name
    , bmc.cnt_author_cited_in_book
    , bmm.mad
    , ABS(bmc.cnt_author_cited_in_book - bmm.mad) / CASE WHEN bmm.mad = 0 THEN 1 ELSE bmm.mad END as ratio
FROM book_metrics_cte bm
INNER JOIN book_metrics_mad_cte bmm
ON bm.dblp_key = bmm.dblp_key
LEFT OUTER JOIN book_member_cited_cte bmc
ON bm.dblp_key = bmc.dblp_key
order by ratio desc
limit 50;


