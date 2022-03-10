{{ config(
  materialized = 'table'
) }}
SELECT
    ROW_NUMBER() OVER (ORDER BY title) as id
  , href
  , title
  , 'TODO' as status
FROM {{ source('elsevier', 'journals')}}