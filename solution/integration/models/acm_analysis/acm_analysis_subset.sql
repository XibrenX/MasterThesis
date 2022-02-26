
SELECT 
editor_name
,dblp_journal_abbreviations as journal_keys
,article_publication_year as publication_year
,COUNT(DISTINCT author_dblp_person_key) FILTER (WHERE ind_article_in_journal = 0) as count_distinct_coauthor_out_journal
,COUNT(DISTINCT author_dblp_person_key) FILTER (WHERE ind_article_in_journal = 1) as count_distinct_coauthor_in_journal
,COUNT(DISTINCT article_title) FILTER (WHERE ind_article_in_journal = 0) as count_distinct_articles_out_journal
,COUNT(DISTINCT article_title) FILTER (WHERE ind_article_in_journal = 1) as count_distinct_articles_in_journal
,COUNT(DISTINCT article_title) as count_distinct_articles
FROM {{ ref('acm_analysis_set') }}
WHERE ind_editor_is_author = 0
GROUP BY editor_name, dblp_journal_abbreviations, publication_year