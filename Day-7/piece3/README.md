# Day 7 — Set Operations from a Spec

## Objective

Practice translating business questions into SQL using `UNION`, `INTERSECT`, and `EXCEPT`.

Database: `Day7Quotes`

## 1. Authors with quotes but no tags

### SQL

```sql
SELECT a.Name AS Author
FROM Authors a
INNER JOIN Quotes q
    ON q.AuthorId = a.Id

EXCEPT

SELECT a.Name AS Author
FROM Authors a
INNER JOIN Quotes q
    ON q.AuthorId = a.Id
INNER JOIN QuoteTags qt
    ON qt.QuoteId = q.Id;
```

### Result

```text
Author
--------------
George Orwell
```

**Operator:** `EXCEPT`

**Why:** It returns authors who have quotes but are not present in the set of authors with tagged quotes.

---

## 2. Authors in both the `classic` and `modern` sets

### SQL

```sql
SELECT DISTINCT a.Name AS Author
FROM Authors a
INNER JOIN Quotes q
    ON q.AuthorId = a.Id
INNER JOIN QuoteTags qt
    ON qt.QuoteId = q.Id
INNER JOIN Tags t
    ON t.Id = qt.TagId
WHERE t.Category = 'classic'

INTERSECT

SELECT DISTINCT a.Name AS Author
FROM Authors a
INNER JOIN Quotes q
    ON q.AuthorId = a.Id
INNER JOIN QuoteTags qt
    ON qt.QuoteId = q.Id
INNER JOIN Tags t
    ON t.Id = qt.TagId
WHERE t.Category = 'modern';
```

### Result

```text
Author
----------------
Albert Einstein
Maya Angelou
Oscar Wilde
```

**Operator:** `INTERSECT`

**Why:** It returns only authors that appear in both the classic and modern sets.

---

## 3. Combined distinct tag list across two categories

### SQL

```sql
SELECT Name AS Tag
FROM Tags
WHERE Category = 'classic'

UNION

SELECT Name AS Tag
FROM Tags
WHERE Category = 'modern'
ORDER BY Tag;
```

### Result

```text
Tag
------------
Innovation
Leadership
Literature
Philosophy
Technology
Wisdom
```

**Operator:** `UNION`

**Why:** It combines the two tag sets and removes duplicate values.

---

## What did you learn this session?

I learned how `UNION`, `INTERSECT`, and `EXCEPT` can translate business requirements into set-based SQL queries.

## What would break this?

Changes to table relationships, category values, column names, or missing tag relationships could produce incorrect or incomplete results.
