SELECT a.[$_object_id]
, a.[$_object_type]
, a.[key] as 'dblp_key'
, a.[publtype]
, t.title
, y.[year]
, j.[journal]
, v.[volume]
, p.pages
, n.number
FROM {{ source('dblp', 'article' ) }} a
LEFT OUTER JOIN {{ source('dblp', 'title' ) }} t
	ON a.[$_object_type] = t.[$_parent_object_type]
	AND a.[$_object_id] = t.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'year' ) }} y
	ON a.[$_object_type] = y.[$_parent_object_type]
	AND a.[$_object_id] = y.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'journal' ) }} j
	ON a.[$_object_type] = j.[$_parent_object_type]
	AND a.[$_object_id] = j.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'volume' ) }} v
	ON a.[$_object_type] = v.[$_parent_object_type]
	AND a.[$_object_id] = v.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'pages' ) }} p
	ON a.[$_object_type] = p.[$_parent_object_type]
	AND a.[$_object_id] = p.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'number' ) }} n
	ON a.[$_object_type] = n.[$_parent_object_type]
	AND a.[$_object_id] = n.[$_parent_object_id]
