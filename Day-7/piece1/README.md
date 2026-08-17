# Day 7 — Joins and CTEs at Depth

## Objective

Practice SQL Server JOINs and Common Table Expressions (CTEs) using a real relational schema.

This exercise covers:

* INNER JOIN
* LEFT JOIN
* CROSS JOIN
* Non-recursive CTEs
* Recursive CTEs
* Window functions
* Aggregation
* Finding the most recent record per author

## Database

SQL Server 2025 Express

Database:

`Day7Quotes`

Tables:

* `Authors`
* `Quotes`
* `AuthorHierarchy`

---

## Main Exercise

The goal was to return each author with:

* Quote count
* Most recent quote
* Most recent quote date

The query uses CTEs and does not use a correlated subquery in the `SELECT` clause.

### SQL

```sql
WITH QuoteCounts AS
(
    SELECT
        AuthorId,
        COUNT(*) AS QuoteCount
    FROM Quotes
    GROUP BY AuthorId
),
RankedQuotes AS
(
    SELECT
        Id,
        AuthorId,
        Text,
        CreatedAt,
        ROW_NUMBER() OVER
        (
            PARTITION BY AuthorId
            ORDER BY CreatedAt DESC
        ) AS RowNum
    FROM Quotes
)
SELECT
    a.Name AS Author,
    COALESCE(qc.QuoteCount, 0) AS QuoteCount,
    rq.Text AS MostRecentQuote,
    rq.CreatedAt AS MostRecentQuoteDate
FROM Authors a
LEFT JOIN QuoteCounts qc
    ON a.Id = qc.AuthorId
LEFT JOIN RankedQuotes rq
    ON a.Id = rq.AuthorId
    AND rq.RowNum = 1
ORDER BY a.Name;
```

## Result Set

The database contains 6 authors, so the complete result set contains 6 rows:

| Author          | Quote Count | Most Recent Quote                                                     | Most Recent Quote Date |
| --------------- | ----------: | --------------------------------------------------------------------- | ---------------------- |
| Albert Einstein |           3 | The important thing is not to stop questioning.                       | 2026-08-15             |
| George Orwell   |           1 | In a time of deceit, telling the truth is a revolutionary act.        | 2026-07-30             |
| Mark Twain      |           2 | Kindness is a language which the deaf can hear and the blind can see. | 2026-08-05             |
| Maya Angelou    |           2 | You may not control all the events that happen to you.                | 2026-08-14             |
| Nikola Tesla    |           0 | NULL                                                                  | NULL                   |
| Oscar Wilde     |           3 | Always forgive your enemies; nothing annoys them so much.             | 2026-08-16             |

## Why a CTE over a correlated subquery?

A CTE separates the aggregation and ranking logic into reusable query steps, making the query easier to read and avoiding a repeated correlated lookup for each author.

## JOIN Exercises

### INNER JOIN

Used to return only authors that have matching quotes.

### LEFT JOIN

Used to return every author, including authors with no quotes. Nikola Tesla demonstrates this case with a quote count of `0`.

### CROSS JOIN

Used to generate every possible author/quote combination. With 6 authors and 11 quotes, this produces 66 combinations.

## Recursive CTE

A recursive CTE was used to traverse an author hierarchy containing parent-child relationships.

The hierarchy demonstrated levels from 0 through 3.

## What did you learn this session?

I learned how different JOIN types affect which rows are returned, how CTEs can break complex SQL into readable stages, and how `ROW_NUMBER()` with `PARTITION BY` can be used to find the most recent row for each group.

## What would break this?

The query would need changes if the schema changed, such as removing the `AuthorId` relationship, renaming the required columns, storing dates incorrectly, or allowing duplicate timestamps without a deterministic tie-breaker when identifying the most recent quote.

## Files

* `day7-joins-ctes.sql` — SQL queries and exercises
* `README.md` — Exercise documentation and results
