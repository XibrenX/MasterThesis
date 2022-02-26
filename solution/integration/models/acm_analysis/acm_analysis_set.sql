{{ config(
    materialized = 'table',
    tags = ["acm"]
) }}
WITH cln_editorial_board AS (
    -- 657
    -- Naam en journal van editors met een unieke id
    select 
        MD5(name || '^' || journal) as editorial_board_id
      , name
      , journal
    from {{ source('acm', 'editorial_boards') }}
    where role in ('Associate Editors', 'Editors-in-Chief', 'Editor-in-Chief')
    group by name, journal
    order by name, journal
)
, acm_dblp_abbr as (
    -- Journal en afkorting zoals ze bekend staan in DBLP
    select 
        acm_journal
      , ARRAY_AGG(dblp_abbreviation) as dblp_abbr
    from {{ ref('journal_dblp') }} abbr
    GROUP BY acm_journal
)
, dblp_person as (
    -- De persoon zoals deze bekend staat in DBLP
    select dblp_name, dblp_unique_person_key
    from {{ ref('stg_dblp_person') }}
)
select 
    eb.name as editor_name
  , eb.journal
  , abbr.dblp_abbr as dblp_journal_abbreviations
  , dblp_editor.dblp_unique_person_key as editor_dblp_person_key
  , dblp_editor_article.dblp_object_key as article_dblp_object_key
  , substring(dblp_editor_article.dblp_object_key, '\/(\w+)\/') as article_journal_abbriviation
  , dblp_article_author.dblp_unique_person_key as author_dblp_person_key
  , article.dblp_year as article_publication_year
  , article.dblp_journal as article_journal
  , article.dblp_title as article_title
  , CASE
      WHEN dblp_editor.dblp_unique_person_key = dblp_article_author.dblp_unique_person_key THEN 1
      ELSE 0
    END as ind_editor_is_author
  , CASE
      WHEN substring(dblp_editor_article.dblp_object_key, '\/(\w+)\/') = ANY (abbr.dblp_abbr) THEN 1
      ELSE 0
    END as ind_article_in_journal
from cln_editorial_board eb
left outer join acm_dblp_abbr abbr
ON eb.journal = abbr.acm_journal
-- 657
-- De DBLP entry van de editor
left outer join dblp_person dblp_editor
on eb.name = dblp_editor.dblp_name
-- De relatie naar artikelen waar de editor ook author van een article is is
-- 85728
left outer join {{ ref('stg_dblp_article_person') }} dblp_editor_article
on dblp_editor.dblp_unique_person_key = dblp_editor_article.dblp_unique_person_key
-- De relatie naar de authors
left outer join {{ ref('stg_dblp_article_person') }} dblp_article_author
on dblp_editor_article.dblp_object_key = dblp_article_author.dblp_object_key
-- informatie over het artikel
left outer join {{ ref('stg_dblp_article') }} article
on dblp_editor_article.dblp_object_key = article.dblp_object_key

