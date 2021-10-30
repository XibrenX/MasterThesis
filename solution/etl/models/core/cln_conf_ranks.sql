SELECT "Title" as "title"
     , "Acronym" as "acronym"
	 , "Rank" as "ranking"
	 , NULLIF("DBLP", 'none') as "dblp_url"
FROM {{ source('core', 'conf_ranks') }} cr
WHERE "Rank" IN ('A*', 'A', 'B')