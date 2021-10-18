/****** Script for SelectTopNRows command from SSMS  ******/
SELECT [ee_type], count(*) as 'cnt'
  FROM [study].[dblp_clean].[ee_clean]
  where [$_parent_object_type] IN ('article', 'incollection', 'inproceedings')
  and [$_parent_object_id] NOT IN (SELECT [$_parent_object_id] FROM [dblp_clean].[doi] GROUP BY [$_parent_object_id])
  group by ee_type
  order by cnt desc;

SELECT TOP 1000 *
  FROM [study].[dblp_clean].[ee_clean]
  where [$_parent_object_type] IN ('article', 'incollection', 'inproceedings')
  and ee_type = 'wikidata'
  and [$_parent_object_id] NOT IN
  (SELECT [$_parent_object_id] FROM [dblp_clean].[doi] GROUP BY [$_parent_object_id])



/****** Script for SelectTopNRows command from SSMS  ******/
SELECT REPLACE(REPLACE(substring(ee, 1, charindex('/', ee, 9)-1), 'http://', ''), 'https://', '') as x, count(*) as 'cnt'

  FROM [study].[dblp_clean].[ee_clean]
  where [$_parent_object_type] IN ('article', 'incollection', 'inproceedings')
  and ee_type = 'unknown'
  and ee NOT LIKE 'db/%'
  and ee NOT LIKE 'ftp://%'
  AND ee not like '%.pdf'
  and [$_parent_object_id] NOT IN (SELECT [$_parent_object_id] FROM [dblp_clean].[doi] GROUP BY [$_parent_object_id])
  group by 
  REPLACE(REPLACE(substring(ee, 1, charindex('/', ee, 9)-1), 'http://', ''), 'https://', '')
  order by cnt desc

SELECT [$_parent_object_id], [ee]
FROM [study].[dblp_clean].[ee_clean]
WHERE ee LIKE '%dl.acm.org%'
and [$_parent_object_type] IN ('article', 'incollection', 'inproceedings')
and [$_parent_object_id] NOT IN (SELECT [$_parent_object_id] FROM [dblp_clean].[doi] GROUP BY [$_parent_object_id])



