// See https://aka.ms/new-console-template for more information
using Neo4j.Driver;
using Neo4j.Interface;

Console.WriteLine("Start!");

// Query Performance

var personName = "Steve Benford"; // Person X

var par = new Dictionary<string, object>() { ["personName"] = personName };

int MAXDEPTH = int.Parse(ReadConfig.Config["CYCLE_MAX_DEPTH"]);

await QueryPerformance.Run(
new Query($"MATCH (a:Person) WHERE a.dblpName = $personName WITH collect(a) as nodes CALL apoc.nodes.cycles(nodes, {{maxDepth: {MAXDEPTH * 2}}}) YIELD path RETURN path", par),
new Query($"MATCH p=(a:Person)-[*2..{MAXDEPTH * 2}]-(a) WHERE a.dblpName = $personName RETURN p", par),
new Query($"MATCH p=(a:Person)-[*0..{MAXDEPTH}]-() WHERE a.dblpName = $personName RETURN p", par),
new Query($"MATCH (a:Person)-[*0..{MAXDEPTH - 1}]-()-[r]-(n) WHERE a.dblpName = $personName RETURN r,n", par),
new Query($"MATCH (a:Person) WHERE a.dblpName = $personName WITH a CALL apoc.path.subgraphAll(a, {{maxLevel: {MAXDEPTH}}}) YIELD nodes, relationships RETURN nodes, relationships", par),
    new Query(@$"
MATCH (a:Person) WHERE a.dblpName = $personName WITH ID(a) AS i 
CALL gds.bfs.stream('project', {{sourceNode: i, maxDepth: {MAXDEPTH}}}) YIELD nodeIds 
MATCH (n) WHERE ID(n) in nodeIds
WITH collect(n) as n, nodeIds
MATCH (n1)-[r]->(n2) WHERE ID(n1) in nodeIds AND ID(n2) in nodeIds
RETURN n, collect(r)
", par));
