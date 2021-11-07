select  surrogate_id
,orc
,email
,name
,REGEXP_REPLACE(unaccent("name"), '[^A-Za-z\s]*', '', 'g') as "clean_name"
from springer_lncs.bus_author
order by name
LIMIT 1000;

