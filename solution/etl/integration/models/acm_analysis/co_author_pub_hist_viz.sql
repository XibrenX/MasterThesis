{{ config(
    materialized = 'table',
    tags = ["acm"]
) }}

select 
    editor_name, journal, article_publication_year, author_dblp_person_key, ind_article_in_journal, COUNT(*) as cnt_articles
from {{ ref('acm_analysis_set') }}
where 1=1
and ind_editor_is_author = 0
group by editor_name, journal, article_publication_year, author_dblp_person_key, ind_article_in_journal
