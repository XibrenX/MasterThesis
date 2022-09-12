This folder contains the program code for the cycle detections.

- `direct.py` conatins the oldest POC code for cycle detection, by requesting cycles of persons with Python
- We then moded to the C# project `Cycles.Old` for post processing Neo4j cycle paths, (method 1b in the thesis)
- After that we wrote a new C# project `Cycles` that contains the new post processing by first requesting a graph. (Methods 2 in the thesis, code executes 2b)
- `Cycles.Statistics` detects all cycles for all persons (Chapter 8 / Apendix A.3 of the thesis), the data of this program is then read by `after.py` to provide the CSV files that provided the table in Appendix A.3.

- `CoAuthors` was a side project to maybe do something with co-author analysis, but quickly loosed interested in that, since the results were not leading me into a promissing direction.