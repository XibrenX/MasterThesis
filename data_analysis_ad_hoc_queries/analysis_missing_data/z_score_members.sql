with book_members as (
    /*
     * Number of members per book
     */
    SELECT 
        b."$_dblp_key" as dblp_key
      , count(*) FILTER (WHERE role = 'program committee') as "cnt_program_committee_members"
      , count(*) FILTER (WHERE role = 'program chair') as "cnt_program_chair_members"
      , COUNT(*) "cnt_members"
    FROM springer_lncs.book b
    LEFT OUTER JOIN lncs_front_matter.stg_lncs_fm_member_book m
    ON CONCAT(REPLACE(b."$_dblp_key", '/', '_'), '.pdf') = m.book_filename
    WHERE m.role IS NOT NULL
    GROUP BY b."$_dblp_key", m.role
)
, book_articles as (
    /*
     * Number of articles per book
     */
    SELECT 
        b."$_dblp_key" as dblp_key
      , COUNT(*) FILTER (WHERE c."$_book_url" IS NOT NULL) as cnt_articles
    FROM springer_lncs.book b
    left outer join springer_lncs.chapter c
    on b."$_book_url" = c."$_book_url"
    GROUP BY b."$_dblp_key"
)

, book_chapter_author_cte AS (
    /*
     * Full set book -> chapter -> author
     */
    SELECT 
        b."$_dblp_key" as dblp_key
      , a.name as author_name
    FROM springer_lncs.book b
    left outer join springer_lncs.chapter c
    on b."$_book_url" = c."$_book_url"
    left outer join springer_lncs.author a
    on c."$_paper_url" = a."$_paper_url"
)
, member_authoring as (
    /*
     * Number of articles written per member per book
     */
    SELECT 
        dblp_key
      , member_name
      , COUNT(*) FILTER (WHERE author_name IS NOT NULL) as "cnt_articles_written_in_book"
    FROM
    (
        SELECT 
            FIRST_VALUE(bca.dblp_key) OVER (
                PARTITION BY m.book_filename
                ORDER BY CASE WHEN bca.dblp_key IS NOT NULL THEN 0 ELSE 1 END ASC
                ) as dblp_key
          , m.member_name
          , bca.author_name
        FROM "lncs_front_matter"."stg_lncs_fm_member_book" m
        LEFT OUTER JOIN book_chapter_author_cte bca
        ON m.member_name = bca.author_name
        AND m.book_filename = CONCAT(REPLACE(bca.dblp_key, '/', '_'), '.pdf')
    ) x
    GROUP BY dblp_key, member_name
    ORDER BY "cnt_articles_written_in_book" DESC
)
, book_metrics as (
    SELECT
        dblp_key
      , COUNT(*) as "p_members"
      , MIN(cnt_articles_written_in_book) as "x_min"
      , STDDEV(cnt_articles_written_in_book) as "x_stddev"
      , MAX(cnt_articles_written_in_book) as "x_max"
      , AVG(cnt_articles_written_in_book) as "x_avg"
      , PERCENTILE_CONT(0.5) WITHIN GROUP(ORDER BY cnt_articles_written_in_book) as median
    FROM member_authoring
    -- WHERE cnt_articles_written_in_book > 0
    GROUP BY dblp_key
)
, book_metrics_mad as (
    SELECT 
        bm.dblp_key
      , PERCENTILE_CONT(0.5) WITHIN GROUP(ORDER BY ABS(bm.median - ma.cnt_articles_written_in_book)) as mad
    FROM member_authoring ma
    left outer JOIN book_metrics bm
    on ma.dblp_key = bm.dblp_key   
    GROUP BY bm.dblp_key
)
SELECT bm.*
    , ma.member_name
    , ma.cnt_articles_written_in_book
    , bmm.mad
    , ABS(ma.cnt_articles_written_in_book - bmm.mad) / CASE WHEN bmm.mad = 0 THEN 1 ELSE bmm.mad END as ratio
FROM book_metrics bm
INNER JOIN book_metrics_mad bmm
ON bm.dblp_key = bmm.dblp_key
LEFT OUTER JOIN member_authoring ma
ON bm.dblp_key = ma.dblp_key
WHERE ma.cnt_articles_written_in_book > 0
and ABS(ma.cnt_articles_written_in_book - bmm.mad) / CASE WHEN bmm.mad = 0 THEN 1 ELSE bmm.mad END > 3
order by ratio desc;

