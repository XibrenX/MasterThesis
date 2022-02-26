SELECT [$_object_id]
    , [$_object_type]
    , [dblp_key]
    , [publtype]
    , title
    , NULL as 'booktitle'
    , [year]
    , [journal]
    , [volume]
    , pages
    , [number]
FROM {{ ref('article') }}

UNION ALL

SELECT [$_object_id]
    , [$_object_type]
    , [dblp_key]
    , NULL as 'publtype'
    , title
    , booktitle
    , [year]
    , NULL as 'journal'
    , NULL as 'volume'
    , pages
    , NULL as 'number'
FROM {{ ref('incollection') }}

UNION ALL

SELECT [$_object_id]
    , [$_object_type]
    , [dblp_key]
    , NULL as 'publtype'
    , title
    , booktitle
    , [year]
    , NULL as 'journal'
    , NULL as 'volume'
    , pages
    , NULL as 'number'
FROM {{ ref('inproceedings') }}