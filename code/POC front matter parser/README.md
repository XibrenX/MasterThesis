# PDF Parser
This application is built to parse the lncs front matters PDFs.

VSCode (or other editor) with Java plugins and Java development runtime is required to run this project.

In this folder there are also queries listed for:

The run_performance_metrics_view.sql and validation_view.sql can be used to deduct the same parse tree for front matters as Westerbaan. Note that you may have to enlarge the query timeout, since these queries are not very optimized.

The pc_members.sql contains the queries to select the PC members and meta information gathered for the thesis.

The pc_members.cql imports the pc_members csv acquired as output from the pc_members.sql query into Neo4J