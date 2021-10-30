{{ 
    config(materialized='table') 
}}

WITH total_cte AS (
    SELECT 
          run_id
        , '# files total' as "metrics"
        , COUNT(*) as "count"
    FROM {{ source('lncs_front_matter', 'file') }}
    GROUP BY run_id
)
, file_cte AS (
    SELECT run_id, LOWER(CONCAT('# files ', "status", ' (', "message", ')')) as "metric", COUNT(*) as "count"
    FROM
    (
        SELECT run_id
        , "status"
        , CASE 
            WHEN "status" = 'SUCCEEDED' THEN 'OK'
            WHEN "message" IN ('null', '') THEN 'Unknown'
            WHEN "message" LIKE '%organisation%' THEN "message"
            ELSE 'Internal error'
        END as "message"
        FROM {{ source('lncs_front_matter', 'file') }}
    ) x
    GROUP BY run_id, "status", "message"
)
, files_with_interesting_sections_cte AS (
    SELECT run_id, CASE WHEN ind_interested = 1 THEN '# files with interesting sections' ELSE '# files without interesting sections' END as "metric", count(*) as "count"
    FROM (
        SELECT run_id, file_id, MAX(section_interested) as "ind_interested"
        FROM lncs_front_matter.validation
        WHERE file_pass = 1
        GROUP BY run_id, file_id
    ) x 
    GROUP BY run_id, x.ind_interested
)
, interesting_sections_cte AS  (
    SELECT run_id
         , CASE WHEN section_interested = 1 THEN '# sections of interest' ELSE '# sections not of interest' END as "metric"
         , COUNT(*) as "count"
    FROM {{ ref('validation') }}
    WHERE file_pass = 1
    GROUP BY run_id, section_interested
)
, files_parseable_sections_cte AS (
    SELECT run_id
         , CASE 
             WHEN contains_interesting_parseable_sections = 1 THEN '# files with interesting parseable sections'
             ELSE '# files with interesting non-parseable sections'
           END as "message"
         , SUM(y.file_count) as "count"
    FROM (
        SELECT run_id
             , validation_pass_count
             , count(*) as "file_count"
             , CASE 
                 WHEN SUM(validation_pass_count) > 0 THEN 1
                 ELSE 0
               END as "contains_interesting_parseable_sections"
        FROM (
            SELECT run_id, file_id
            , COUNT(*) as "count_validations"
            , SUM(CASE WHEN section_pass = 'PASS' THEN 1 ELSE 0 END) as "validation_pass_count"
            FROM lncs_front_matter.validation
            WHERE file_pass = 1 AND section_interested = 1
            GROUP BY run_id, file_id
        ) x 
        GROUP BY x.run_id, validation_pass_count
    ) y
    GROUP BY y.run_id, contains_interesting_parseable_sections
)
, interesting_parseable_sections_cte AS (
    SELECT run_id
         , CASE 
             WHEN section_pass = 'PASS' THEN '# interesting parseable sections'
             ELSE '# interesting non-parseable sections'
           END as "metric"
         , COUNT(*) as "count"
    FROM {{ ref('validation') }}
    WHERE file_pass = 1 
      AND section_interested = 1
    GROUP BY run_id, section_pass
)
, interesting_extractable_members_cte AS (
    SELECT run_id
         , CONCAT('# members of interest extractable (', "clean_role", ')') as "metric"
         , SUM(num_members) as "count"
    FROM {{ ref('validation') }}
    WHERE file_pass = 1
      AND section_pass = 'PASS'
      AND section_interested = 1
      AND "clean_role" <> '-'
    GROUP BY run_id, clean_role
)

SELECT *
FROM total_cte

UNION ALL

SELECT *
FROM file_cte

UNION ALL

SELECT *
FROM files_with_interesting_sections_cte

UNION ALL

SELECT *
FROM interesting_sections_cte

UNION ALL
SELECT *
FROM files_parseable_sections_cte

UNION ALL
SELECT *
FROM interesting_parseable_sections_cte

UNION ALL
SELECT *
FROM interesting_extractable_members_cte