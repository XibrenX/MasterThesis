{{ 
    config(materialized='ephemeral') 
}}
SELECT 
        "filename"
    , {{ lncs_front_matter_member_role() }} as "role"
    , CASE 
        WHEN firstname <> 'null' and lastname <> 'null' THEN CONCAT(TRIM(firstname), ' ', TRIM(lastname)) 
        ELSE
            TRIM(SPLIT_PART(
                CASE
                    WHEN TRIM("name") LIKE '%,' THEN TRIM(REPLACE("name", ',', ''))
                    ELSE "name" 
                END, '(', 1)
            ) 
    END as "name"
FROM {{ source('lncs_front_matter', 'file') }} f
LEFT OUTER JOIN {{ source('lncs_front_matter', 'section') }} s
    ON f.id = s.file_id
LEFT OUTER JOIN {{ source('lncs_front_matter', 'member') }} m
    ON s.id = m.section_id
INNER JOIN {{ ref('validation') }} v
    ON f.id = v.file_id
    AND s.id = v.section_id
    AND v.section_interested = 1
    AND v.section_pass = 'PASS'
    AND v.file_pass = 1
WHERE f.run_id = (SELECT MAX(run_id) FROM {{ source('lncs_front_matter', 'file') }})
