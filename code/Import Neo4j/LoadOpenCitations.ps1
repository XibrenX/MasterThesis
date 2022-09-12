. "../read_config.ps1"

$username = $Config["NEO4J_USERNAME"]
$password = $Config["NEO4J_PASSWORD"]
$cypershell = $Config["NEO4J_CYPHER_SHELL"]
$import = $Config["NEO4J_IMPORT"]

$files = Get-ChildItem -Path $Config["OPENCITATIONS_IMPORT"] -Recurse -Filter *.csv

$OutputEncoding = [System.Text.Encoding].UnicodeEncoding

foreach ($f in $files) {
	$fN = $f.Name
	$fF = $f.FullName
	$fi = $import + "\" + $f.FullName
	cmd.exe /c mklink "$fi" "$fF"
	
	$script = "USING PERIODIC COMMIT 5000 LOAD CSV WITH HEADERS FROM 'file:///$fN' AS row WITH row MATCH (:DOI {d: row.citing})-->(as:Article), (:DOI {d: row.cited})-->(ad:Article) CREATE (as)-[:cites {Oci: row.oci, Creation: row.creation, Timespan: row.timespan, JournalSC: row.journal_sc, AuthorSC: row.author_sc, from_oc: true}]->(ad) RETURN count(*);"
	
	Invoke-Expression "$cypershell -u $username -p $password --format plain -f `"$script`""
	
	cmd.exe /c del /F /Q "$fi"
}