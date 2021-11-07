{{ 
    config(materialized='table') 
}}
WITH member_role_cte AS (
SELECT
      run_id
    , id
    , section_id
    , {{ lncs_front_matter_member_role() }} as "clean_role"
FROM {{ source('lncs_front_matter', 'member') }}
)
, section_members_count_cte AS (
    SELECT run_id, section_id, COUNT(*) as "num_members"
    FROM {{ source('lncs_front_matter', 'member') }}
    GROUP BY run_id, section_id
)
, section_validation_cte AS (
    SELECT 
      s.run_id
    , s.id
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

    FROM  {{ source('lncs_front_matter', 'section') }} s
    LEFT OUTER JOIN section_members_count_cte m
    ON s.id = m.section_id
    AND s.run_id = m.run_id
)
, interested_sections_cte AS (
    SELECT 
          s.run_id
        , s.id as "section_id"
        , m.clean_role
        , MAX(CASE WHEN m.clean_role IN ('program committee') THEN 1 ELSE 0 END) OVER (PARTITION BY s.run_id, s.id) as "ind_interest"
        , COUNT(m.*) as "num_members"
    FROM {{ source('lncs_front_matter', 'section') }} s
    LEFT OUTER JOIN member_role_cte m
    ON s.id = m.section_id
    AND s.run_id = m.run_id
    GROUP BY s.id, m.clean_role
)
, file_section_cte AS (
    SELECT 
           f.run_id
         , f.id as "file_id"
         , COUNT(s.*) as "num_sections"
    FROM lncs_front_matter.file f
    LEFT OUTER JOIN {{ source('lncs_front_matter', 'section') }} s
    ON f.id = s.file_id
    GROUP BY f.id
)
, file_cte AS (
    SELECT
          f.run_id
        , f.id as "file_id"
        , CASE WHEN fs.num_sections = 0 OR "status" = 'FAILED' THEN 0 ELSE 1 END as "ind_pass"
        , CASE WHEN fs.num_sections = 0 AND f.message IS NULL THEN 'No sections' ELSE f.message END as "message"
    FROM {{ source('lncs_front_matter', 'file') }} f
    LEFT OUTER JOIN file_section_cte fs
    ON f.id = fs.file_id
    AND f.run_id = fs.run_id
)

    SELECT f.run_id, f.file_id, f.ind_pass as "file_pass", intsec.ind_interest as "section_interested", secval.validation as "section_pass", f.message, intsec.section_id, intsec.clean_role, intsec.num_members
    FROM file_cte f
    LEFT OUTER JOIN section_validation_cte secval
        ON f.file_id = secval.file_id
        AND f.ind_pass = 1
        AND f.run_id = secval.run_id
    LEFT OUTER JOIN interested_sections_cte intsec
        ON secval.id = intsec.section_id
        AND secval.run_id = intsec.run_id