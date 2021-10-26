WITH member_role_cte AS (
SELECT  id
    ,   CASE
            WHEN 
                    LOWER("role") LIKE '%program%' 
                AND LOWER("role") LIKE '%chair%'  
                AND LOWER("role") NOT LIKE '%co-%'
            THEN 'program chair' 

            WHEN 
                    LOWER("role") LIKE '%program%' 
                AND LOWER("role") LIKE '%committee%'  
            THEN 'program committee'

            WHEN 
                    LOWER("role") LIKE '%review%'
                OR  LOWER("role") LIKE '%referee%'
            THEN 'reviewer'

            WHEN 
                    LOWER("role") LIKE '%steering committee%'
            THEN 'steering committee'

            WHEN 
                    LOWER("role") LIKE '%member%'
            THEN 'program committee'
            
            ELSE '-'
        END as "clean_role"
FROM lncs_front_matter.member
)
, section_members_count_cte AS (
    SELECT section_id, COUNT(*) as "num_members"
    FROM lncs_front_matter.member
    GROUP BY section_id
)
, section_validation_cte AS (
    SELECT 
      s.id
    , CASE 
        WHEN s.parser = 'ZeroParser'
        THEN
          CASE
            WHEN num_members IS NULL
              THEN 'PASS'
            ELSE 'FAIL'
          END
        WHEN s.parser = 'Two_Name_Affiliation'
        THEN
          CASE
            WHEN s.num_section_lines_non_empty - s.num_merged_lines = num_members
              THEN 'PASS' 
            ELSE 'FAIL'
          END
        WHEN s.parser = 'TwoLastNameFirstNameParser'
        THEN
          CASE
            WHEN 2 * (s.num_section_lines_non_empty - s.num_merged_lines) <= m.num_members
              THEN 'PASS'
            ELSE 'FAIL'
          END
        WHEN s.parser = 'ThreeLastNameFirstNameParser'
        THEN
          CASE
            WHEN 3 * (s.num_section_lines_non_empty - s.num_merged_lines) <= m.num_members
              THEN 'PASS'
            ELSE 'FAIL'
          END
        WHEN s.parser = 'Two_Role_NameAff'
        THEN
          CASE
            WHEN s.num_section_lines_non_empty - s.num_merged_lines = m.num_members
              THEN 'PASS'
            ELSE 'FAIL'
          END
        WHEN s.parser = 'OneColumnNameAffiliation'
        THEN
          CASE
            WHEN s.num_section_lines_non_empty - s.num_merged_lines = m.num_members
              THEN 'PASS'
            ELSE 'FAIL'
          END
        WHEN s.parser = 'OneColumnOddNameEvenAffiliation'
        THEN
          CASE 
            WHEN (s.num_section_lines_non_empty - s.num_merged_lines) / 2 = m.num_members
              THEN 'PASS'
            ELSE 'FAIL'
          END
        WHEN s.parser = 'ThreeLastFirstAffParser'
        THEN
          CASE 
            WHEN s.num_section_lines_non_empty - s.num_merged_lines = m.num_members
              THEN 'PASS'
            ELSE 'FAIL'
          END
        ELSE 'UNKNOWN'
      END as "validation"

    FROM  lncs_front_matter.section s
    LEFT OUTER JOIN section_members_count_cte m
    ON s.id = m.section_id
)
, section_cte as (
    SELECT s.*, mc.num_members, sv.validation
    FROM lncs_front_matter.section s
    LEFT OUTER JOIN section_members_count_cte mc
    ON s.id = mc.section_id
    LEFT OUTER JOIN section_validation_cte sv
    ON s.id = sv.id
)
, member_cte as (
    SELECT m.*
    , mr.clean_role
    , CASE 
        WHEN mr.clean_role IN ('program committee')
            THEN 1 
        ELSE 0 END as "ind_interest"
    FROM lncs_front_matter.member m
    LEFT OUTER JOIN member_role_cte mr
    ON m.id = mr.id
)
, file_cte as (
    SELECT 
          f.*
        , CASE 
            WHEN f.message <> 'No organisation position found' 
            THEN 'Error processing file'
            ELSE f.message
        END as "error_description"
    FROM lncs_front_matter.file f 
)
, file_metrics as ()
SELECT s.file_id
    , s.validation
    FROM section_cte s
    ON f.id = s.file_id
    LEFT OUTER JOIN member_cte m
    ON s.id = m.section_id
)

LIMIT 100;



-- SELECT *
-- FROM lncs_front_matter.file f 
-- LEFT OUTER JOIN lncs_front_matter.section s
-- ON f.id = s.file_id
-- LEFT OUTER JOIN member m
-- ON s.id = m.section_id
-- WHERE m.clean_role IN ('program committee')
-- LIMIT 100;

