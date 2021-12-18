{{ config(
    materialized = 'table',
    tags = ["acm"]
) }}
with x_cte as (
    select 
        editor_name, journal, article_publication_year, author_dblp_person_key, ind_article_in_journal, COUNT(*) as cnt_articles
    from {{ ref('acm_analysis_set') }}
    where 1=1
    and ind_editor_is_author = 0
    group by editor_name, journal, article_publication_year, author_dblp_person_key, ind_article_in_journal
)
SELECT editor_name, journal, article_publication_year, cnt_articles, ind_article_in_journal, COUNT(DISTINCT author_dblp_person_key) as cnt_unique_coauthor
FROM x_cte
GROUP BY editor_name, journal, article_publication_year, cnt_articles, ind_article_in_journal
ORDER BY editor_name, article_publication_year desc