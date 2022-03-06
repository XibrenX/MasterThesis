# Elsevier

This directory contains applications to get the necessary data from Elsevier.

## Journal scraper

Gets all journals from Elsevier.
Information we retrieve is title, issn and href to more info about the journal.


## Subdir Elsevier

This subdirectory is a Visual Studio Solution and targets a SQL Server database
(legacy from Research Proposal experiment). To create one `flow', this needs to
be transformed to Postgres and store data to the storage defined in the config 
file.
