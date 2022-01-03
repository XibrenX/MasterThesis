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
    SELECT aj.author_name, d."$_journal"
    , CAST(DATE_PART('YEAR', date_publication) as INTEGER) as year_pub
    , AVG(d.publication_lag) avg_pulication_lag
    FROM elsevier_author_journal_cte aj
    LEFT OUTER JOIN dates_cte d
    on aj."$_id" = d."$_id"
    AND aj."$_journal" = d."$_journal"
    GROUP BY aj.author_name, d."$_journal", year_pub
)
, dblp_info_raw_cte as (
    select p.dblp_name
    , p.dblp_unique_person_key
    , a.dblp_year
    , COUNT(*) as cnt_journal_publications
    , COUNT(DISTINCT substring(ap.dblp_object_key, '\/(\w+)\/')) as cnt_distinct_venues
    from {{ ref('stg_dblp_person') }} p
    left outer join {{ ref('stg_dblp_article_person') }} ap
    on p.dblp_unique_person_key = ap.dblp_unique_person_key
    left outer join {{ ref('stg_dblp_article') }} a
    on ap.dblp_object_key = a.dblp_object_key
    where substring(ap.dblp_object_key, '(\w+)\/')  = 'journals'
    group by p.dblp_name, p.dblp_unique_person_key, a.dblp_year
)
SELECT e.*, d.dblp_unique_person_key, d.cnt_journal_publications, d.cnt_distinct_venues
FROM elsevier_agg_cte e 
LEFT OUTER JOIN dblp_info_raw_cte d
ON e.year_pub = d.dblp_year
AND e.author_name = d.dblp_name
