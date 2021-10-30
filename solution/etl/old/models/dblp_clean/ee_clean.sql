SELECT [$_valid_on]
      ,[$_object_type]
      ,[$_object_id]
      ,[$_parent_object_id]
      ,[$_parent_object_type]
      ,[ee]
	  ,CASE 
	     WHEN [ee] LIKE 'https://doi.org/%' THEN 'doi_doi.org'
		 WHEN [ee] LIKE 'http://doi.ieeecomputersociety.org/%' THEN 'doi_ieee'
		 WHEN [ee] LIKE 'https://dl.acm.org/doi/%' THEN 'doi_acm'
		 WHEN [ee] LIKE 'https://www.wikidata.org/%' THEN 'wikidata'
		 WHEN [ee] LIKE 'https://openreview.net/%' THEN 'openreview'
		 WHEN [ee] LIKE 'https://www.tandfonline.com/doi/abs/%' THEN 'tandfonline'
		 WHEN [ee] LIKE '%arxiv%' THEN 'arxiv'
		 WHEN [ee] LIKE '%ieeexplore.ieee.org%' THEN 'ieee_explore'
	    ELSE 'unknown' 
	  END as 'ee_type'
  FROM {{ source('dblp', 'ee') }}