DROP TABLE lncs_front_matter.validation;

CREATE TABLE lncs_front_matter.validation AS (

WITH member_role_cte AS (
SELECT  id
    , section_id
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
    , s.file_id
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
, interested_sections_cte AS (
    SELECT s.id as "section_id"
        , m.clean_role
        , MAX(CASE WHEN m.clean_role IN ('program committee') THEN 1 ELSE 0 END) OVER (PARTITION BY s.id) as "ind_interest"
        , COUNT(m.*) as "num_members"
    FROM lncs_front_matter.section s
    LEFT OUTER JOIN member_role_cte m
    ON s.id = m.section_id
    GROUP BY s.id, m.clean_role
)
, file_section_cte AS (
    SELECT f.id as "file_id"
         , COUNT(s.*) as "num_sections"
    FROM lncs_front_matter.file f
    LEFT OUTER JOIN lncs_front_matter.section s
    ON f.id = s.file_id
    GROUP BY f.id
)
, file_cte AS (
    SELECT
        id as "file_id"
        , CASE WHEN fs.num_sections = 0 OR "status" = 'FAILED' THEN 0 ELSE 1 END as "ind_pass"
        , CASE WHEN fs.num_sections = 0 AND f.message IS NULL THEN 'No sections' ELSE f.message END as "message"
    FROM lncs_front_matter.file f
    LEFT OUTER JOIN file_section_cte fs
    ON f.id = fs.file_id
)

    SELECT f.file_id, f.ind_pass as "file_pass", intsec.ind_interest as "section_interested", secval.validation as "section_pass", f.message, intsec.section_id, intsec.clean_role, intsec.num_members
    FROM file_cte f
    LEFT OUTER JOIN section_validation_cte secval
        ON f.file_id = secval.file_id
        AND f.ind_pass = 1
    LEFT OUTER JOIN interested_sections_cte intsec
        ON secval.id = intsec.section_id    
)

