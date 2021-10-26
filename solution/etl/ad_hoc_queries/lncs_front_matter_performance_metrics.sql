
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

-- STEP 3
SELECT count_validation_pass, count(*) as "file_count", ROUND((COUNT(*) / SUM(COUNT(*)) OVER ()) * 100, 2) as "percentage"
FROM (
    SELECT file_id, SUM(CASE WHEN section_pass = 'PASS' THEN 1 ELSE 0 END) as "count_validation_pass"
    FROM lncs_front_matter.validation
    WHERE file_pass = 1 AND section_interested = 1
    GROUP BY file_id, CASE WHEN section_pass = 'PASS' THEN 1 ELSE 0 END
) x 
GROUP BY x.count_validation_pass
ORDER BY x.count_validation_pass;

-- SELECT file_id
-- FROM (
--     SELECT file_id, SUM(CASE WHEN section_pass = 'PASS' THEN 1 ELSE 0 END) as "count_validation_pass"
--     FROM lncs_front_matter.validation
--     WHERE file_pass = 1 AND section_interested = 1
--     GROUP BY file_id, CASE WHEN section_pass = 'PASS' THEN 1 ELSE 0 END
-- ) x 
-- WHERE count_validation_pass = 16
-- -- GROUP BY x.count_validation_pass;

-- SELECT COUNT(*)
-- FROM lncs_front_matter.file;