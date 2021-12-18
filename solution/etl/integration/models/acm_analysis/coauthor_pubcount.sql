{{ config(
    materialized = 'table',
    tags = ["acm"]
) }}
    select 
        editor_name
      , author_dblp_person_key as coauthor_dblp_key
      , ind_article_in_journal
      , COUNT(*) as cnt_articles
    from 
        {{ ref('acm_analysis_set') }}
    where ind_editor_is_author = 0
    group by editor_name, author_dblp_person_key, ind_article_in_journal
    order by editor_name, coauthor_dblp_key, ind_article_in_journal