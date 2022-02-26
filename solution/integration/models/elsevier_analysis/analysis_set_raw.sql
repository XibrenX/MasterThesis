{{ config(
    materialized = 'table',
    tags = ["acm"]
) }}
with author_agg_cte as (
    select  
        "$_id"
      , "$_journal"
      , article_author_id
      , MIN(value) FILTER (WHERE property = 'given-name') as given_name
      , MIN(value) FILTER (WHERE property = 'surname') as surname
    from {{ source('elsevier', 'authors') }}
    group by "$_id"
    ,"$_journal"
    ,article_author_id
)
, elsevier_author_journal_cte as (
    select 
        "$_id"
      , "$_journal"
      , article_author_id
      , given_name || ' ' || surname as author_name
    from author_agg_cte
)
, dates_cte as (
    SELECT 
        "$_id"
    , "$_journal"
    , COUNT(*) FILTER (WHERE type = 'Revised') as cnt_revised
    , MIN(CAST(value AS DATE)) FILTER (WHERE type = 'Received') as date_received
    , MIN(CAST(value AS DATE)) FILTER (WHERE type = 'Available online') as date_available_online
    , MIN(CAST(value AS DATE)) FILTER (WHERE type = 'Accepted') as date_accepted
    , MIN(CAST(value AS DATE)) FILTER (WHERE type = 'Publication date') as date_publication
    , MIN(CAST(value AS DATE)) FILTER (WHERE type = 'Accepted') - MIN(CAST(value AS DATE)) FILTER (WHERE type = 'Received') as publication_lag
    FROM {{ source('elsevier', 'dates') }}
    GROUP BY "$_id"
    , "$_journal"
)
, elsevier_agg_cte as (
    SELECT aj."$_id" as id,
     aj.author_name, d."$_journal" as journal
    , CAST(DATE_PART('YEAR', date_publication) as INTEGER) as year_pub
    , d.publication_lag
    FROM elsevier_author_journal_cte aj
    LEFT OUTER JOIN dates_cte d
    on aj."$_id" = d."$_id"
    AND aj."$_journal" = d."$_journal"
)
select *
from elsevier_agg_cte

