{{ config(
    materialized = 'table',
    indexes = [ 
        {'columns': ['dblp_object_key'],'type': 'hash' },
        {'columns': ['dblp_doi'],'type': 'hash' } 
        ],
    tags = ["dblp", "staging"]
) }}

SELECT
    A.id            as dblp_object_id,
    A.object_type   as dblp_object_type,
    A.dblp_key      as dblp_object_key,
    A.publtype      as dblp_publtype,
    e.doi           as dblp_doi
FROM
    {{ ref('cte_scope') }} A
LEFT OUTER JOIN {{ ref('lan_ee') }} e
    ON A.id = e.parent_id
WHERE e.doi IS NOT NULL
GROUP BY 
    A.id           
    ,A.object_type 
    ,A.dblp_key    
    ,A.publtype     
    ,e.doi    