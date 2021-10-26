
WITH members_cte AS (
    SELECT section_id, COUNT(*) as "num_members"
    FROM lncs_front_matter.member
    GROUP BY section_id
)
, metrics_cte AS (
SELECT 
      s.file_id
    , s.num_parts
    , s.num_section_lines_non_empty
    , s.num_merged_lines
    , m.num_members
    , s.parser
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
LEFT OUTER JOIN members_cte m
ON s.id = m.section_id
WHERE f.status = 'SUCCEEDED'
)

SELECT validation, count(*)
from metrics_cte
group by validation;
