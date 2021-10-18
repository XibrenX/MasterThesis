with dblp_cte as (
	SELECT *
	FROM [dblp_clean].[publication] dp
	WHERE [dp].[year] < 2020
)
, dblp_doi_cte as (
	SELECT dp.*, dd.doi
	FROM dblp_cte dp
	left outer join [dblp_clean].[doi] dd
	ON dp.[$_object_id] = dd.[$_parent_object_id]	
)
, aminer_cte as (
	SELECT * FROM
	[aminer].[paper]
)
, match_on_doi_cte as (
	select d.[$_object_id] as 'dblp_id', a.[id] as 'aminer_id'
	from dblp_doi_cte d
	LEFT OUTER JOIN aminer_cte a
	ON d.doi = a.doi
	where a.id IS NOT NULL
	GROUP BY d.[$_object_id], a.[id]
)
, match_on_name as (
	SELECT dp.[$_object_id] as 'dblp_id', a.id as 'aminer_id'
	FROM [dblp_clean].[publication] dp
	LEFT OUTER JOIN aminer_cte a
	ON REPLACE(dp.title, '.', '') = REPLACE(a.title, '.', '')
	WHERE dp.[$_object_id] NOT IN (
		SELECT dblp_id from match_on_doi_cte
	)
	AND a.id IS NOT NULL
)

--SELECT COUNT(*)
--FROM match_on_name

SELECT title, dp.[$_object_type], dp.[year], count(*) as 'cnt'
from dblp_cte dp
where dp.[$_object_id] NOT IN (SELECT dblp_id FROM match_on_doi_cte)
AND dp.[$_object_id] NOT IN (SELECT dblp_id FROM match_on_name)
group by title, dp.[$_object_type], dp.[year]
order by cnt desc

--SELECT COUNT(*)
--FROM match_on_doi_cte

--select count(*)
--from [dblp_clean].[publication] dp
--WHERE [dp].[year] < 2020
