{{ config(
  materialized = 'table',
  indexes = [ 
    { 'columns': ["dblp_name"], 'type': 'hash' },
    { 'columns': ["dblp_unique_person_key"], 'type': 'hash' }
  ],
  tags = ["dblp", "staging"]
) }}

WITH author_cte AS (
  SELECT
      a.author_name
    , FIRST_VALUE(a.orcid) OVER (PARTITION BY a.author_name) as orcid
  FROM {{ ref('lan_author') }} a
  GROUP BY a.author_name, a.orcid
)
, orc_propagate_cte AS (
  SELECT 
      a.author_name as dblp_name
    , FIRST_VALUE(a.orcid) OVER (PARTITION BY COALESCE(w.www_dblp_key, a.author_name)) as dblp_orcid
    , COALESCE(w.www_dblp_key, a.author_name) as dblp_unique_person_key
  FROM author_cte a
  LEFT OUTER JOIN {{ ref('www_cte') }} w
    ON a.author_name = w.author_name
)
SELECT dblp_name, dblp_orcid, dblp_unique_person_key
FROM orc_propagate_cte
GROUP BY dblp_name, dblp_orcid, dblp_unique_person_key


