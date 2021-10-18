SELECT [Title] as 'title'
     , [Acronym] as 'acronym'
	 , [Rank] as 'ranking'
	 , NULLIF([DBLP], 'none') as 'dblp_url'
FROM {{ source('core', 'conf-ranks') }} cr
WHERE cr.[Rank] IN ('A*', 'A', 'B')