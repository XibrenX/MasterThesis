{{ config(
  materialized = 'table',
  tags = ["dblp"]
) }}

SELECT
  MD5(
    s.dblp_key
  ) AS "article_hash_key",
  sa.person_hash_key AS "person_hash_key",
  sa.name,
  A.author_name,
  sa.orcid
FROM
  {{ ref('cte_scope') }}
  s
  LEFT OUTER JOIN {{ ref('lan_author') }} A
  ON s.id = A.parent_id
  LEFT OUTER JOIN {{ ref('stg_person') }}
  sa
  ON A.author_name = sa.name
