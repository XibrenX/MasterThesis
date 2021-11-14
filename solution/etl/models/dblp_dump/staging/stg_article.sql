{{ config(
    materialized = 'table',
        tags=["dblp"]
) }}

SELECT
    A.id,
    A.object_type,
    A.dblp_key AS "article_business_key",
    MD5(
        A.dblp_key
    ) AS "article_hash_key",
    A.publtype,
    t.title,
    y."year",
    j.journal,
    v.volume,
    pages,
    n.number
FROM
    {{ ref('cte_scope') }} A
    LEFT OUTER JOIN {{ ref('lan_title') }}
    t
    ON A.id = t.parent_id
    LEFT OUTER JOIN {{ ref('lan_year') }}
    y
    ON A.id = y.parent_id
    LEFT OUTER JOIN {{ ref(
        'lan_journal'
    ) }}
    j
    ON A.id = j.parent_id
    LEFT OUTER JOIN {{ source(
        'dblp_dump',
        'volume'
    ) }}
    v
    ON A.object_type = v."$_parent_object_type"
    AND A.id = v."$_parent_object_id"
    LEFT OUTER JOIN {{ source(
        'dblp_dump',
        'pages'
    ) }}
    p
    ON A.object_type = p."$_parent_object_type"
    AND A.id = p."$_parent_object_id"
    LEFT OUTER JOIN {{ source(
        'dblp_dump',
        'number'
    ) }}
    n
    ON A.object_type = n."$_parent_object_type"
    AND A.id = n."$_parent_object_id"
