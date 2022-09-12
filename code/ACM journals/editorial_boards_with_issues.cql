
LOAD CSV WITH HEADERS FROM 'file:///editorial_board_issue_merge.csv' AS row
CALL {
    WITH row
    MATCH (p:Person {dblpName: row.name}), (j:Journal {Code: row.journal_code})
	MATCH (vi {wKey: j.wKey + row.wkey} ) WHERE vi:Issue OR vi:Volume
    MERGE (p)-[x:editor_of]->(vi)
    	ON CREATE
			SET 
			x.from_acm = true, 
			x.roles = [row.role]
		ON MATCH
			SET 
			x.from_acm = true, 
			x.roles = coalesce(x.roles, []) + row.role
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);