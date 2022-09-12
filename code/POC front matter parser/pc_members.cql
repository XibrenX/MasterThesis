// 211053 of 294307

LOAD CSV WITH HEADERS FROM 'file:///lncs_w_pc_members.csv' AS row
CALL {
	WITH row
	MATCH (p:Person {dblpName: row.name}), (r:Proceeding {dblpKey: replace(replace(row.filename, '.pdf', ''), '_', '/')})
	MERGE (p)-[x:pc_member_of]->(r)
		ON CREATE
			SET 
			x.from_lncs = true,
			x.roles = [row.role]
		ON MATCH
			SET 
			x.from_lncs = true,
			x.roles = coalesce(x.roles, []) + row.role
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);