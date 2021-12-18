{{ config(
    materialized = 'table',
    tags = ["acm"]
) }}
with core_cte as (
    select 
        acm_name
      , acm_journal
      , acm_role
      , dblp_unique_person_key
      , dblp_object_id
      , dblp_object_key
      , dblp_object_type
      , dblp_abbr_from_key
      , dblp_title
      , dblp_year
      , dblp_journal
      , dblp_volume
      , dblp_pages
      , dblp_number
      , lookup_dblp_journal_abbr
      , ind_publiced_in_journal
      , author_dblp_unique_person_key
      , author_name
      , author_orcid
    from {{ ref('acm_analysis_set')}}
    where acm_role IN ('Associate Editors', 'Editors-in-Chief', 'Editor-in-Chief')
    -- where dblp_object_type = 'article'
)
, counts_cte as (
    select 
        acm_name as member
      , acm_journal
      , dblp_year as pub_year
      , dblp_object_type
      , COUNT(DISTINCT dblp_object_key) as cnt_publications
      , COUNT(DISTINCT dblp_object_key) FILTER (WHERE ind_publiced_in_journal = 1) as cnt_publications_inside_journal
      , COUNT(DISTINCT dblp_object_key) FILTER (WHERE ind_publiced_in_journal = 0) as cnt_publications_outside_journal
      , COUNT(DISTINCT author_dblp_unique_person_key) as cnt_coauthor_total
      , COUNT(DISTINCT author_dblp_unique_person_key) FILTER (WHERE ind_publiced_in_journal = 1) as cnt_coauthors_inside_journal
      , (CAST(COUNT(DISTINCT author_dblp_unique_person_key) FILTER (WHERE ind_publiced_in_journal = 1) as DOUBLE PRECISION) / CAST(COUNT(DISTINCT author_dblp_unique_person_key) as DOUBLE PRECISION)) * 100 as perc_coauthors_inside_journal
      , COUNT(DISTINCT author_dblp_unique_person_key) FILTER (WHERE ind_publiced_in_journal = 0) as cnt_coauthors_outside_journal
      , (CAST(COUNT(DISTINCT author_dblp_unique_person_key) FILTER (WHERE ind_publiced_in_journal = 0) as DOUBLE PRECISION) / CAST(COUNT(DISTINCT author_dblp_unique_person_key) as DOUBLE PRECISION)) * 100 as perc_coauthors_outside_journal
    from core_cte
    where dblp_unique_person_key <> author_dblp_unique_person_key
    group by acm_name, dblp_year, acm_journal, dblp_object_type
)
, journal_year_avg_cte as (
    SELECT
        acm_journal
      , pub_year
      , dblp_object_type
      , AVG(perc_coauthors_inside_journal) as avg_perc_coauthors_inside_journal
      , AVG(perc_coauthors_outside_journal) as avg_perc_coauthors_outside_journal
    FROM counts_cte
    GROUP BY acm_journal, pub_year, dblp_object_type
)
, year_avg_cte as (
    SELECT
        pub_year
      , dblp_object_type
      , AVG(perc_coauthors_inside_journal) as avg_perc_coauthors_inside_journal
      , AVG(perc_coauthors_outside_journal) as avg_perc_coauthors_outside_journal
    FROM counts_cte
    GROUP BY pub_year, dblp_object_type
)
select 
    c.member
  , c.acm_journal
  , c.pub_year
  , c.dblp_object_type
  , c.cnt_coauthor_total
  , c.cnt_coauthors_inside_journal
  , c.perc_coauthors_inside_journal
  , j_avg.avg_perc_coauthors_inside_journal as journal_avg_perc_coauthors_inside_journal
  , y.avg_perc_coauthors_inside_journal as year_avg_perc_coauthors_inside_journal
  , c.perc_coauthors_outside_journal
  , c.cnt_coauthors_outside_journal
  , j_avg.avg_perc_coauthors_outside_journal as journal_avg_perc_coauthors_outside_journal
  , y.avg_perc_coauthors_outside_journal as year_avg_perc_coauthors_outside_journal
  , c.cnt_coauthors_inside_journal - c.cnt_coauthors_outside_journal as cnt_coauthor_diff
  , c.cnt_publications
  , c.cnt_publications_inside_journal
  , c.cnt_publications_outside_journal
from counts_cte c
left outer join journal_year_avg_cte j_avg
on c.pub_year = j_avg.pub_year
and c.acm_journal = j_avg.acm_journal
left outer join year_avg_cte y
on c.pub_year = y.pub_year

-- where c.member = 'Steve Marschner'

--order by c.pub_year desc
