//Load Articles
CREATE INDEX FOR (a:Article) ON (a.dblpKey);

LOAD CSV WITH HEADERS FROM 'file:///5_a_0.csv' AS row
CALL {
	WITH row
	CREATE (:Article {dblpKey: row.dblpKey, Title: row.Title, Year: toInteger(row.Year)})
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Persons
CREATE INDEX FOR (p:Person) ON (p.dblpName);

LOAD CSV WITH HEADERS FROM 'file:///5_p_0.csv' AS row
CALL {
	WITH row
	CREATE (:Person {Name: row.Name, dblpName: row.dblpName})
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Articles_Authors
LOAD CSV WITH HEADERS FROM 'file:///5_aa_0.csv' AS row
CALL {
	WITH row
	MATCH (p:Person {dblpName: row.dblpPersonName}), (a:Article {dblpKey: row.dblpArticleKey})
	MERGE (p)-[r:author_of]->(a)
	ON CREATE
		SET r.from_dblp = true
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Remove double Article - Person relationships
MATCH (:Article)-[:author_of]-(:Person)-[r:author_of]-(:Article) DELETE r RETURN count(*);

LOAD CSV WITH HEADERS FROM 'file:///5_aa_1.csv' AS row
CALL {
	WITH row
	MATCH (p:Person {dblpName: row.dblpPersonName}), (a:Article {dblpKey: row.dblpArticleKey})
	MERGE (p)-[r:author_of]->(a)
	ON CREATE
		SET r.from_dblp = true
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Person_Orcid
LOAD CSV WITH HEADERS FROM 'file:///5_po_0.csv' AS row
CALL {
	WITH row
	MATCH (p:Person {dblpName: row.dblpPersonName})
	CREATE (:Orcid {o: row.Orcid})-[:orcid_of {from_dblp: true}]->(p)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Article_DOI
CREATE INDEX FOR (d:DOI) ON (d.d);

LOAD CSV WITH HEADERS FROM 'file:///5_ad_0.csv' AS row
CALL {
	WITH row
	MATCH (a:Article {dblpKey: row.dblpArticleKey})
	CREATE (:DOI {d: row.DOI})-[:doi_of {from_dblp: true}]->(a)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Conferences
CREATE INDEX FOR (c:Conference) ON (c.dblpKey);

LOAD CSV WITH HEADERS FROM 'file:///5_c_0.csv' AS row
CALL {
	WITH row
	CREATE (:Conference {dblpKey: row.dblpKey, Code: row.Code})
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Proceeding
CREATE INDEX FOR (r:Proceeding) ON (r.dblpKey);

LOAD CSV WITH HEADERS FROM 'file:///5_r_0.csv' AS row
CALL {
	WITH row
	CREATE (r:Proceeding {dblpKey: row.dblpKey, Code: row.Code, Title: row.Title, Year: toInteger(row.Year), Isbn: row.ISBN, Publisher: row.Publisher})
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

// Load Proceeding-Conference
LOAD CSV WITH HEADERS FROM 'file:///5_r_0.csv' AS row
CALL {
	WITH row
	MATCH (r:Proceeding {dblpKey: row.dblpKey}), (c:Conference {dblpKey: split(row.dblpKey, '/')[0] + '/' + split(row.dblpKey, '/')[1]})
	CREATE (r)-[:of {from_dblp: true}]->(c)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Article_Proceeding (Missing 5)
LOAD CSV WITH HEADERS FROM 'file:///5_ar_0.csv' AS row
CALL {
	WITH row
	MATCH (r:Proceeding {dblpKey: row.dblpProceedingKey}), (a:Article {dblpKey: row.dblpArticleKey})
	CREATE (a)-[:belongs_to {from_dblp: true}]->(r)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Proceeding_Editor (Missing 2887)
LOAD CSV WITH HEADERS FROM 'file:///5_re_0.csv' AS row
CALL {
	WITH row
	MATCH (p:Person {dblpName: row.dblpPersonName}), (r:Proceeding {dblpKey: row.dblpProceedingKey})
	CREATE (p)-[:pc_member_of {from_dblp: true}]->(r)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Journal
CREATE INDEX FOR (j:Journal) ON (j.wKey);

LOAD CSV WITH HEADERS FROM 'file:///5_j_0.csv' AS row
CALL {
	WITH row
	CREATE (j:Journal {Code: row.Code, Title: row.Title, wKey: toInteger(row.wKey), Publisher: row.Publisher})
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Journal_Volume
CREATE INDEX FOR (v:Volume) ON (v.wKey);

LOAD CSV WITH HEADERS FROM 'file:///5_jv_0.csv' AS row
CALL {
	WITH row
	OPTIONAL MATCH (j:Journal {wKey: toInteger(split(row.wKey, '#')[0])})
	CREATE (v:Volume {v: split(row.wKey, '#')[1], wKey: row.wKey })
	WITH v,j WHERE j IS NOT NULL
	CREATE (v)-[:of {from_dblp: true}}]->(j)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Journal_Volume_Issue (Missing 179 relationships)
CREATE INDEX FOR (i:Issue) ON (i.wKey);

LOAD CSV WITH HEADERS FROM 'file:///5_jvi_0.csv' AS row
CALL {
	WITH row
	OPTIONAL MATCH (jv {wKey: split(row.wKey, '#')[0] + '#' + split(row.wKey, '#')[1]}) WHERE jv:Journal OR jv:Volume
	CREATE (i:Issue {i: split(row.wKey, '#')[2], wKey: row.wKey })
	WITH i,jv WHERE jv IS NOT NULL
	CREATE (i)-[:of {from_dblp: true}]->(jv)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Load Article_Journal (Missing 50)
LOAD CSV WITH HEADERS FROM 'file:///5_aj_0.csv' AS row
CALL {
	WITH row
	MATCH (jvi {wKey: row.wJournalKey }), (a:Article {dblpKey: row.dblpArticleKey}) WHERE jvi:Journal OR jvi:Volume OR jvi:Issue
	CREATE (a)-[:belongs_to {from_dblp: true}]->(jvi)
} IN TRANSACTIONS OF 5000 ROWS
RETURN count(*);

//Remove double belongs_to relationships
MATCH (:Proceeding)-[:belongs_to]-(:Article)-[r:belongs_to]-() DELETE r RETURN count(*);

//Remove "empty" Issues, Volumes, Journals
MATCH (i:Issue) WHERE NOT exists((:Article)-[:belongs_to]->(i)) DETACH DELETE i RETURN count(*);

MATCH (v:Volume) WHERE NOT exists((:Article)-[:belongs_to]->(v)) AND NOT exists((:Issue)-[:of]->(v)) DETACH DELETE v RETURN count(*);

MATCH (j:Journal) WHERE NOT exists((:Article)-[:belongs_to]->(j)) AND NOT exists((:Volume)-[:of]->(j)) DETACH DELETE j RETURN count(*);
