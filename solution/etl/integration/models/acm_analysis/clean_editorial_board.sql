{{ config(
    materialized = 'table',
    tags = ["acm"]
) }}
select ROW_NUMBER() OVER (ORDER BY name, journal) as editorial_board_id, name, journal
from {{ source('acm', 'editorial_boards') }}
where role in ('Associate Editors', 'Editors-in-Chief', 'Editor-in-Chief')
group by name, journal
order by name, journal