
-- First decision
WITH core AS (
SELECT 
      CASE WHEN MIN(file_pass) = 0 THEN 'FAIL' ELSE 'PASS' END as "status"
    , CASE 
        WHEN MIN(file_pass) = 1 THEN 'OK'
        WHEN STRING_AGG("message", '|') IN('null', '') THEN 'Unknown'
        WHEN STRING_AGG("message", '|') LIKE '%organisation%' THEN STRING_AGG("message", '|')
        ELSE 'Internal error'
      END as "message"
FROM lncs_front_matter.validation
GROUP BY file_id
)

SELECT "status", "message", COUNT(*) as "file_count", ROUND((COUNT(*) / SUM(COUNT(*)) OVER ()) * 100, 2) as "percentage"
FROM core
GROUP BY "status", "message"
ORDER BY "status" DESC, "file_count" ASC;


-- STEP 2
SELECT ind_interested, count(*) as "file_count", ROUND((COUNT(*) / SUM(COUNT(*)) OVER ()) * 100, 2) as "percentage"
FROM (
    SELECT file_id, MAX(section_interested) as "ind_interested"
    FROM lncs_front_matter.validation
    WHERE file_pass = 1
    GROUP BY file_id
) x 
GROUP BY x.ind_interested;

SELECT section_interested, COUNT(*)
FROM lncs_front_matter.validation
WHERE file_pass = 1
GROUP BY section_interested;

-- STEP 3
SELECT CASE WHEN y.count_validation_pass > 0 THEN 1 ELSE 0 END as "contains_passed_sections"
, SUM(y.file_count) as "file_count"
FROM (
    SELECT count_validation_pass
    , count(*) as "file_count"
    , ROUND((COUNT(*) / SUM(COUNT(*)) OVER ()) * 100, 2) as "percentage"
    ,SUM(COUNT(*)) OVER ()
    FROM (
        SELECT file_id
        , COUNT(*) as "count_validations"
        , SUM(CASE WHEN section_pass = 'PASS' THEN 1 ELSE 0 END) as "count_validation_pass"
        FROM lncs_front_matter.validation
        WHERE file_pass = 1 AND section_interested = 1
        GROUP BY file_id
    ) x 
    GROUP BY x.count_validation_pass
) y
GROUP BY CASE WHEN y.count_validation_pass > 0 THEN 1 ELSE 0 END
ORDER BY "contains_passed_sections" DESC;

SELECT section_pass, COUNT(*)
FROM lncs_front_matter.validation
WHERE file_pass = 1 AND section_interested = 1
GROUP BY section_pass;