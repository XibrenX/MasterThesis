{{ config(
  materialized = 'table',
  tags = ["dblp"]
) }}

WITH ee_clean_cte AS (

  SELECT
    "$_parent_object_id",
    "$_parent_object_type",
    ee,CASE
      WHEN ee LIKE 'https://doi.org/%' THEN 'doi_doi.org'
      WHEN ee LIKE 'http://doi.ieeecomputersociety.org/%' THEN 'doi_ieee'
      WHEN ee LIKE 'https://dl.acm.org/doi/%' THEN 'doi_acm'
      WHEN ee LIKE 'https://www.wikidata.org/%' THEN 'wikidata'
      WHEN ee LIKE 'https://openreview.net/%' THEN 'openreview'
      WHEN ee LIKE 'https://www.tandfonline.com/doi/abs/%' THEN 'tandfonline'
      WHEN ee LIKE '%arxiv%' THEN 'arxiv'
      WHEN ee LIKE '%ieeexplore.ieee.org%' THEN 'ieee_explore'
      ELSE 'unknown'
    END AS "ee_type"
  FROM
    {{ source(
      'dblp_dump',
      'ee'
    ) }}
),
doi_cte AS (
  SELECT
    "$_parent_object_id",
    "$_parent_object_type",
    ee AS "doi_link",
    ee_type AS "link_type",
    CASE
      WHEN ee_type = 'doi_doi.org' THEN REPLACE(
        ee,
        'https://doi.org/',
        ''
      )
      WHEN ee_type = 'doi_ieee' THEN REPLACE(
        ee,
        'http://doi.ieeecomputersociety.org/',
        ''
      )
      WHEN ee_type = 'doi_acm' THEN REPLACE(REPLACE(ee, 'https://dl.acm.org/doi/', ''), 'abs/', '')
      WHEN ee_type = 'tandfonline' THEN REPLACE(
        ee,
        'https://www.tandfonline.com/doi/abs/',
        ''
      )
    END AS "doi"
  FROM
    ee_clean_cte
  WHERE
    ee_type IN (
      'doi_doi.org',
      'doi_ieee',
      'doi_acm',
      'tandfonline'
    )
)
SELECT
  A.dblp_key,
  MD5(
    A.dblp_key
  ) AS article_hash_key,
  d.doi_link,
  d.doi
FROM
  {{ ref('cte_scope') }} A
  LEFT OUTER JOIN doi_cte d
  ON A.id = d."$_parent_object_id"
  AND A.object_type = d."$_parent_object_type"
WHERE
  d.doi IS NOT NULL
