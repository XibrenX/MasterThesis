with core_cte as (
    select 
        *,
        substring(article_dblp_object_key, '(\w+)\/')
    from acm_analysis.acm_analysis_set
    where substring(article_dblp_object_key, '(\w+)\/') = 'journals'
    -- and dblp_object_id = 5466188
    -- limit 100
)
, counts_cte as (
    select 
        editor_name
      , journal
      , article_publication_year as pub_year
      , COUNT(DISTINCT article_dblp_object_key) as cnt_publications
      , COUNT(DISTINCT article_dblp_object_key) FILTER (WHERE ind_article_in_journal = 1) as cnt_publications_inside_journal
      , COUNT(DISTINCT article_dblp_object_key) FILTER (WHERE ind_article_in_journal = 0) as cnt_publications_outside_journal
      , COUNT(DISTINCT author_dblp_person_key) as cnt_coauthor_total
      , COUNT(DISTINCT author_dblp_person_key) FILTER (WHERE ind_article_in_journal = 1) as cnt_coauthors_inside_journal
      , (CAST(COUNT(DISTINCT author_dblp_person_key) FILTER (WHERE ind_article_in_journal = 1) as DOUBLE PRECISION) / NULLIF(CAST(COUNT(DISTINCT author_dblp_person_key) as DOUBLE PRECISION), 0)) * 100 as perc_coauthors_inside_journal
      , COUNT(DISTINCT author_dblp_person_key) FILTER (WHERE ind_article_in_journal = 0) as cnt_coauthors_outside_journal
      , (CAST(COUNT(DISTINCT author_dblp_person_key) FILTER (WHERE ind_article_in_journal = 0) as DOUBLE PRECISION) / NULLIF(CAST(COUNT(DISTINCT author_dblp_person_key) as DOUBLE PRECISION), 0)) * 100 as perc_coauthors_outside_journal
    from core_cte
    where ind_editor_is_author = 0
    group by editor_name, article_publication_year, journal
)
, journal_year_avg_cte as (
    SELECT
        journal
      , pub_year
      , AVG(cnt_coauthors_inside_journal) as avg_cnt_coauthors_inside_journal
      -- , AVG(perc_coauthors_inside_journal) as avg_perc_coauthors_inside_journal
      , STDDEV(cnt_coauthors_inside_journal) as stdev_cnt_coauthors_inside_journal
      , AVG(cnt_coauthors_outside_journal) as avg_cnt_coauthors_outside_journal
      -- , AVG(perc_coauthors_outside_journal) as avg_perc_coauthors_outside_journal
    FROM counts_cte
    GROUP BY journal, pub_year
)
, year_avg_cte as (
    SELECT
        pub_year
      , AVG(cnt_coauthors_inside_journal) as avg_cnt_coauthors_inside_journal
      , AVG(perc_coauthors_inside_journal) as avg_perc_coauthors_inside_journal
      , AVG(cnt_coauthors_outside_journal) as avg_cnt_coauthors_outside_journal
      , AVG(perc_coauthors_outside_journal) as avg_perc_coauthors_outside_journal
    FROM counts_cte
    GROUP BY pub_year
)
, analysis_set as (
select 
    c.editor_name
  , c.journal
  , c.pub_year
  , c.cnt_coauthor_total
  , c.cnt_coauthors_inside_journal
  , j_avg.avg_cnt_coauthors_inside_journal as jnl_avg_coauthor_inside
  , j_avg.stdev_cnt_coauthors_inside_journal as jnl_stdev_coauthor_inside
  , (c.cnt_coauthors_inside_journal - j_avg.avg_cnt_coauthors_inside_journal) / NULLIF(j_avg.stdev_cnt_coauthors_inside_journal, 0) as z_score
  -- , c.perc_coauthors_inside_journal
  -- , j_avg.avg_perc_coauthors_inside_journal as journal_avg_perc_coauthors_inside_journal
  , y.avg_perc_coauthors_inside_journal as year_avg_perc_coauthors_inside_journal
  -- , c.perc_coauthors_outside_journal
  , c.cnt_coauthors_outside_journal
  -- , j_avg.avg_perc_coauthors_outside_journal as journal_avg_perc_coauthors_outside_journal
  -- , y.avg_perc_coauthors_outside_journal as year_avg_perc_coauthors_outside_journal
  , c.cnt_coauthors_inside_journal - c.cnt_coauthors_outside_journal as cnt_coauthor_diff
  , c.cnt_publications
  , c.cnt_publications_inside_journal
  , c.cnt_publications_outside_journal
from counts_cte c
left outer join journal_year_avg_cte j_avg
on c.pub_year = j_avg.pub_year
and c.journal = j_avg.journal
left outer join year_avg_cte y
on c.pub_year = y.pub_year
)
-- where c.editor_name = 'Steve Marschner'

-- select *
-- from analysis_set
-- -- where z_score > 2
-- -- GROUP BY editor_name
-- order by cnt_publications_inside_journal desc nulls last

select *
from analysis_set
where editor_name = 'Yongfeng Zhang'
order by pub_year desc -- nulls last

-- Steve Benford
;
with x_cte as (
    select 
        editor_name, journal, article_publication_year, author_dblp_person_key, ind_article_in_journal, COUNT(*) as cnt_articles
    from acm_analysis.acm_analysis_set
    where 1=1
    -- and editor_name = 'Steve Marschner'
    and article_publication_year = 2020
    and ind_editor_is_author = 0
    and substring(article_dblp_object_key, '(\w+)\/') = 'journals'
    group by editor_name, journal, article_publication_year, author_dblp_person_key, ind_article_in_journal
)
SELECT editor_name, journal, article_publication_year, ind_article_in_journal, SUM(cnt_articles) as sum_cnt_articles, COUNT(DISTINCT author_dblp_person_key) as cnt_unique_coauthor
FROM x_cte
where journal = 'ACM Transactions on Information Systems'
-- and ind_article_in_journal = 1
GROUP BY editor_name, journal, article_publication_year, ind_article_in_journal, ind_article_in_journal
ORDER BY editor_name, ind_article_in_journal;