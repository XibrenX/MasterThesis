{{ config(
  materialized = 'table',
  tags = ["dblp", "staging"],
  indexes = [ 
        {'columns': ['dblp_object_key'],'type': 'hash' },
        {'columns': ['dblp_unique_person_key'],'type': 'hash' } 
      ],
) }}

SELECT
    s.dblp_key as dblp_object_key
  , sa.dblp_unique_person_key
FROM
  {{ ref('cte_scope') }} s
LEFT OUTER JOIN {{ ref('lan_author') }} la
  ON s.id = la.parent_id
LEFT OUTER JOIN {{ ref('stg_dblp_person') }} sa
  ON la.author_name = sa.dblp_name
GROUP BY s.dblp_key, sa.dblp_unique_person_key
