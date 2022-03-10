SELECT
    href
  , title
  , 'TODO' as status
FROM {{ source('elsevier', 'journals')}}