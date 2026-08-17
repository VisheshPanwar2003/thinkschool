# Day 7 — Window Functions

## Objective

Practice SQL Server window functions including `ROW_NUMBER`, `RANK`, `LAG`, `LEAD`, and running totals using `SUM() OVER()`.

## Exercise

Return each quote per author with:

* Running quote count
* Gap in days since the author's previous quote

## SQL

```sql
USE Day7Quotes;
GO

SELECT
    a.Name AS Author,
    q.Text AS Quote,
    q.CreatedAt,

    SUM(1) OVER
    (
        PARTITION BY q.AuthorId
        ORDER BY q.CreatedAt
        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
    ) AS RunningQuoteCount,

    DATEDIFF
    (
        DAY,
        LAG(q.CreatedAt) OVER
        (
            PARTITION BY q.AuthorId
            ORDER BY q.CreatedAt
        ),
        q.CreatedAt
    ) AS DaysSincePreviousQuote

FROM Quotes q
INNER JOIN Authors a
    ON a.Id = q.AuthorId

ORDER BY
    a.Name,
    q.CreatedAt;
```

## Result Set

```text
Author             Quote                                      CreatedAt                    RunningQuoteCount    DaysSincePreviousQuote
Albert Einstein    Life is like riding a bicycle...           2026-08-01 10:00:00          1                    NULL
Albert Einstein    Imagination is more important than...      2026-08-10 14:30:00          2                    9
Albert Einstein    The important thing is not to stop...      2026-08-15 09:15:00          3                    5
George Orwell      In a time of deceit, telling the truth... 2026-07-30 10:30:00          1                    NULL
Mark Twain         The secret of getting ahead...             2026-07-20 11:00:00          1                    NULL
Mark Twain         Kindness is a language which...            2026-08-05 16:00:00          2                    16
Maya Angelou       Try to be a rainbow in someone's cloud.   2026-08-03 09:00:00          1                    NULL
Maya Angelou       You may not control all the events...     2026-08-14 15:00:00          2                    11
Oscar Wilde        Be yourself; everyone else is already...  2026-07-25 13:00:00          1                    NULL
Oscar Wilde        Experience is simply the name...           2026-08-12 18:00:00          2                    18
Oscar Wilde        Always forgive your enemies...             2026-08-16 12:00:00          3                    4
```

> Note: The quote text is shortened with `...` above for readability. The actual SQL result contains the complete quote text.

## What did you learn this session?

I learned how window functions calculate values across related rows without grouping them, and how `LAG()` and `SUM() OVER()` can be used for previous-row comparisons and running totals.

## What would break this?

Changes to the author relationship, column names, or missing/invalid `CreatedAt` values could produce incorrect ordering and day-gap calculations.
