1. Clustered Index — Id

Index DDL:

CREATE CLUSTERED INDEX CX_IndexTest_Id
ON dbo.IndexTest(Id);

Query:

SELECT *
FROM dbo.IndexTest
WHERE Id BETWEEN 50000 AND 50100;

Logical Reads:

Before index: 1281
After index: 5

The clustered index reduced the logical reads significantly for the Id range query.


2. Non-Clustered Index — AuthorId

Index DDL:

CREATE NONCLUSTERED INDEX IX_IndexTest_AuthorId
ON dbo.IndexTest(AuthorId);

Query:

SELECT *
FROM dbo.IndexTest
WHERE AuthorId = 500;

Logical Reads:

Before index: 536
After index: 317

The non-clustered AuthorId index reduced the logical reads by allowing SQL Server to locate matching rows more efficiently.


3. Non-Clustered Index — Category

Index DDL:

CREATE NONCLUSTERED INDEX IX_IndexTest_Category
ON dbo.IndexTest(Category);

Query:

SELECT *
FROM dbo.IndexTest
WHERE Category = 'technology';

Logical Reads:

Before index: 1286
After index: 1286

The Category index did not reduce logical reads because the query returned 25,000 rows and SQL Server determined that scanning the table was more efficient than using the non-clustered index.


### Write-side cost

Indexes improve read performance but add storage and write overhead because SQL Server must maintain the indexes during INSERT, UPDATE, and DELETE operations.