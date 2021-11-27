select  "$_object_type"
,"$_object_id"
,"$_parent_object_id"
,"$_parent_object_type"
,ee
,type
from dblp_dump.ee
where ee like '%10.1007/978-3-642-20407-4_4%'
LIMIT 10;

select *
from integration.article_s_dblp
where title = 'GP-Based Electricity Price Forecasting.';

