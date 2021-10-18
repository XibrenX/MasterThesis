SELECT a.[$_object_id]
, a.[$_object_type]
, a.[key] as 'dblp_key'
, t.title
, bt.booktitle
, u.[type] as 'url_type'
, u.[url]
, y.[year]
, p.pages
FROM {{ source('dblp', 'inproceedings' ) }} a
LEFT OUTER JOIN {{ source('dblp', 'title' ) }} t
	ON a.[$_object_type] = t.[$_parent_object_type]
	AND a.[$_object_id] = t.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'booktitle' ) }} bt
	ON a.[$_object_type] = bt.[$_parent_object_type]
	AND a.[$_object_id] = bt.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'url' ) }} u
	ON a.[$_object_type] = u.[$_parent_object_type]
	AND a.[$_object_id] = u.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'year' ) }} y
	ON a.[$_object_type] = y.[$_parent_object_type]
	AND a.[$_object_id] = y.[$_parent_object_id]
LEFT OUTER JOIN {{ source('dblp', 'pages' ) }} p
	ON a.[$_object_type] = p.[$_parent_object_type]
	AND a.[$_object_id] = p.[$_parent_object_id]
