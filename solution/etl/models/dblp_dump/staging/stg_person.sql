{{ config(
  materialized = 'table',
  indexes = [ { 'columns': ["name"],
  'type': 'hash' }],
  tags = ["dblp"]
) }}

SELECT
  A.author_name AS "name",
  A.orcid,
  COALESCE(
    w.person_business_key,
    A.author_name
  ) AS person_business_key,
  COALESCE(w.person_hash_key, MD5(A.author_name)) AS person_hash_key
FROM
  {{ ref('lan_author') }} A
  LEFT OUTER JOIN {{ ref('www_cte') }}
  w
  ON A.author_name = w.author_name
WHERE
  A.parent_type <> 'www'
UNION ALL
SELECT
  author_name AS "name",
  NULL AS "orcid",
  person_business_key,
  person_hash_key
FROM
  {{ ref('www_cte') }}
