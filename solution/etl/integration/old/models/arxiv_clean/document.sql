SELECT [entry_id]
      ,[updated]
      ,[published]
      ,[title]
      ,NULLIF([journal_ref], 'None') as 'journal_ref'
      ,NULLIF([doi], 'None') as 'doi'
      ,[$_dblp_ee]
      ,[$_api_extract_dts]
      ,[$_api_status]
      ,[$_error_message]
  FROM {{ source('arxiv', 'document') }}