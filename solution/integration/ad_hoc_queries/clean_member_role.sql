SELECT  *
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
FROM lncs_front_matter.member;