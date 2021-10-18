SELECT [$_valid_on]
      ,[$_object_type]
      ,[$_object_id]
      ,[$_parent_object_id]
      ,[$_parent_object_type]
	  ,ee as 'doi_link'
	,CASE
	WHEN ee_type = 'doi_doi.org' THEN REPLACE(ee, 'https://doi.org/', '')
	WHEN ee_type = 'doi_ieee' THEN REPLACE(ee, 'http://doi.ieeecomputersociety.org/', '')
	WHEN ee_type = 'doi_acm' THEN REPLACE(REPLACE(ee, 'https://dl.acm.org/doi/', ''), 'abs/', '')
	WHEN ee_type = 'tandfonline' THEN REPLACE(ee, 'https://www.tandfonline.com/doi/abs/', '') 
	END as 'doi'
FROM {{ ref('ee_clean') }}
WHERE ee_type IN ('doi_doi.org', 'doi_ieee', 'doi_acm', 'tandfonline')

UNION

SELECT [$_valid_on]
      ,[$_object_type]
      ,[$_object_id]
      ,[$_parent_object_id]
      ,[$_parent_object_type]
	  ,ee as 'doi_link'
	  , a.[doi] as 'doi'
FROM {{ ref('ee_clean') }} d
LEFT OUTER JOIN {{ ref('document') }} a
	ON d.[ee] = a.[$_dblp_ee]
	AND a.[$_api_status] = 'SUCCEEDED'
WHERE d.ee_type = 'arxiv'
  AND a.[doi] IS NOT NULL

UNION

SELECT d.[$_valid_on]
      ,d.[$_object_type]
      ,d.[$_object_id]
      ,d.[$_parent_object_id]
      ,d.[$_parent_object_type]
      ,d.[ee] as 'doi_link'
      ,w.doi as 'doi'
  FROM {{ ref('ee_clean') }} d
  INNER JOIN {{ ref('wikidata') }} w
  ON d.ee = w.url
  where d.ee_type = 'wikidata'

UNION

SELECT d.[$_valid_on]
      ,d.[$_object_type]
      ,d.[$_object_id]
      ,d.[$_parent_object_id]
      ,d.[$_parent_object_type]
      ,d.[ee] as 'doi_link'
      ,w.doi as 'doi'
  FROM {{ ref('ee_clean') }} d
  INNER JOIN {{ source('ieee_explore', 'document') }} w
  ON d.[$_parent_object_id] = w.[$_dblp_document_id]
  where d.ee_type = 'ieee_explore'
  AND w.[doi] IS NOT NULL