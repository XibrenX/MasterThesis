# Code

## Config file
There is a config-example file, make a copy and rename it to config. Fill it with the correct values.
You have to replace the values indicated with '<' and '>'.

## Folder structure:

<pre>
<b>code</b>
<b>├ ACM Journals:</b> Python scripts to scrape, dowload and parse the ACM journal overview pages, editorial boards, issue pages, and issue front matters. (Sectie 5.4 en 5.5 of the thesis)
<b>├ Cycles:</b> Pyton scripts and C# projects, to detect cycles
<b>  ├ Cycles:</b> C# Project to detect cycles for persons
<b>  ├ Cycles.Statistics:</b> C# Project to detect all cycles for all persons (Chapter 8 of the thesis)
<b>  └ after.py:</b> Python script to parse to sum the output of Cycles.Statistics (for Appendix A.3 of the thesis)
<b>├ database:</b> Generic database python package developed by @woudw, used for exporting ACM Journals results to Postgres database
<b>├ DBLPtoCSV:</b> Converts the DBLP xml to several CSV files to be importend into Neo4j (Sectie 6.1 of the thesis)
<b>├ Import Neo4J:</b> Contains Powershell scripts to (re-)import all CSV files from all different datasources into Neo4j. Reset_db.ps1 completly cleans and (re-)imports the Neo4j database. Also contains the Cypher queries to import the DBLP CSV files into Neo4j.
<b>├ Neo4j interface:</b> Contains the performance comparison (of Table 7.1 of the thesis), and also contains debug code to efficiently remove many nodes or edges from Neo4j.
<b>├ Outliers:</b> Two python files that perform the outlier detection (of Chapter 9)
<b>├ POC front matter parser:</b> The front matter parser for LNCS files developed by @woudw, improved by me
<b>└ root files:</b> the root files are to read the config in different programming languages, and a tor.py file that can do Tor or delays and randomizes network traffic to scrape the ACM website. It only worked without Tor, but with delays.
</pre>



## Tools used
VSCode was used for all projects except the C# projects, which are developed in Visual Studio.
HeidiSQL for querying the Postgres database and outputting CSVs from it.
Neo4j, browser and bloom for inserting, testing Cypher queries, and analysing found results.

## Python packages used
- BeautifulSoup
- requests
- stem 
- numpy
- matplotlib
- termcolor
- neo4j