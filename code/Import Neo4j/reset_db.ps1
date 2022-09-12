$ErrorActionPreference = "Stop"

. "../read_config.ps1"

$username = $Config["NEO4J_USERNAME"]
$password = $Config["NEO4J_PASSWORD"]
$cypershell = $Config["NEO4J_CYPHER_SHELL"]

$cmd = "$cypershell -u $username -p $password --format plain -f "

Invoke-Expression ($cmd + "reset_db.cql")

Invoke-Expression ($cmd + "Import_DBLP_Csvs.cql") > "Import_DBLP_Csvs_log.txt"

Invoke-Expression ($cmd + "../LNCSPdfParser/pc_members.cql") > "pc_members_log.txt"

Invoke-Expression ($cmd + "../ACM Journals/editorial_boards_with_issues.cql") > "editorial_boards_with_issues_log.txt"

./LoadOpenCitations.ps1 > "OpenCitations_log.txt"
