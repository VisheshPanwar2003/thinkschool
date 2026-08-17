USE Day7Quotes;
GO

-- =====================================================
-- DAY 7: JOINS AND CTEs
-- =====================================================


-- =====================================================
-- 1. INNER JOIN
-- Returns authors who have at least one quote
-- =====================================================

SELECT
    a.Id AS AuthorId,
    a.Name AS Author,
    q.Id AS QuoteId,
    q.Text AS Quote,
    q.CreatedAt
FROM Authors a
INNER JOIN Quotes q
    ON a.Id = q.AuthorId
ORDER BY a.Name, q.CreatedAt;


-- =====================================================
-- 2. LEFT JOIN
-- Returns every author, including authors with no quotes
-- =====================================================

SELECT
    a.Id AS AuthorId,
    a.Name AS Author,
    q.Id AS QuoteId,
    q.Text AS Quote
FROM Authors a
LEFT JOIN Quotes q
    ON a.Id = q.AuthorId
ORDER BY a.Name;


-- =====================================================
-- 3. CROSS JOIN
-- Produces every possible author/quote combination
-- =====================================================

SELECT
    a.Name AS Author,
    q.Id AS QuoteId
FROM Authors a
CROSS JOIN Quotes q;


-- =====================================================
-- 4. NON-RECURSIVE CTE: Quote Count
-- =====================================================

WITH QuoteCounts AS
(
    SELECT
        AuthorId,
        COUNT(*) AS QuoteCount
    FROM Quotes
    GROUP BY AuthorId
)
SELECT *
FROM QuoteCounts;


-- =====================================================
-- 5. NON-RECURSIVE CTE: Rank Quotes by Recency
-- =====================================================

WITH RankedQuotes AS
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
SELECT *
FROM RankedQuotes
ORDER BY AuthorId, RowNum;


-- =====================================================
-- 6. MAIN DAY 7 REQUIREMENT
-- Each author + quote count + most recent quote
-- Uses CTEs and JOINs.
-- No correlated subquery in SELECT.
-- =====================================================

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


-- =====================================================
-- 7. RECURSIVE CTE
-- Demonstrates hierarchical data traversal
-- =====================================================

WITH AuthorHierarchyCTE AS
(
    -- Anchor member
    SELECT
        a.Id,
        a.Name,
        h.ParentAuthorId,
        0 AS Level
    FROM Authors a
    INNER JOIN AuthorHierarchy h
        ON a.Id = h.AuthorId
    WHERE h.ParentAuthorId IS NULL

    UNION ALL

    -- Recursive member
    SELECT
        child.Id,
        child.Name,
        h.ParentAuthorId,
        parent.Level + 1
    FROM AuthorHierarchyCTE parent
    INNER JOIN AuthorHierarchy h
        ON h.ParentAuthorId = parent.Id
    INNER JOIN Authors child
        ON child.Id = h.AuthorId
)
SELECT
    Id,
    Name,
    ParentAuthorId,
    Level
FROM AuthorHierarchyCTE
ORDER BY Level, Name;