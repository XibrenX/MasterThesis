SELECT "Title" as "title"
	 , "Rank" as "ranking"
	 , "DBLP" as dblp_url
	 , REPLACE("DBLP", 'https://dblp.uni-trier.de/db/', '') as dblp_key
FROM {{ source('core', 'jnl_ranks') }} x
WHERE "Rank" IN ('A*', 'A', 'B')
AND "DBLP" IS NOT NULL
