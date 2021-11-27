{{ config(
    materialized = 'table',
    schema = 'tempory_tables',
    indexes = [ { 'columns': ["author_name"], 'type': 'hash' } ],
    tags = ["dblp", "staging"]
) }}

WITH unique_author_cte AS (

    SELECT
        author_name,
        parent_id
    FROM
        {{ ref('lan_author') }}
    WHERE
        parent_type = 'www'
    GROUP BY
        parent_id,
        author_name
)
SELECT
    A.author_name
  , w.dblp_key AS www_dblp_key
FROM unique_author_cte A
LEFT OUTER JOIN {{ ref('lan_www') }} w
  ON A.parent_id = w.id
