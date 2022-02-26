{{ config(
    materialized = 'table',
    indexes = [ 
        {'columns': ['dblp_object_key'],'type': 'hash' },
        {'columns': ['dblp_title'],'type': 'hash' }
     ],
    tags = ["dblp", "staging"]
) }}

SELECT
    A.id            as dblp_object_id,
    A.object_type   as dblp_object_type,
    A.dblp_key      as dblp_object_key,
    A.publtype      as dblp_publtype,
    t.title         as dblp_title,
    y."year"        as dblp_year,
    j.journal       as dblp_journal,
    v.volume        as dblp_volume,
    pages           as dblp_pages,
    n.number        as dblp_number
FROM
    {{ ref('cte_scope') }} A
LEFT OUTER JOIN {{ ref('lan_title') }} t
    ON A.id = t.parent_id
LEFT OUTER JOIN {{ ref('lan_year') }} y
    ON A.id = y.parent_id
LEFT OUTER JOIN {{ ref('lan_journal') }} j
    ON A.id = j.parent_id
LEFT OUTER JOIN {{ ref('lan_volume') }} v
    ON A.id = v.parent_id
LEFT OUTER JOIN {{ ref('lan_pages') }} p
    ON A.id = p.parent_id
LEFT OUTER JOIN {{ ref('lan_number') }} n
    ON A.id = n.parent_id
