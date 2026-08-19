-- ============================================================
-- DAY 9 — ISOLATION LEVELS + READ ANOMALIES
-- ============================================================

USE Day7Quotes;
GO


-- ============================================================
-- 1. Dirty Read
-- ============================================================

-- Session 1

BEGIN TRANSACTION;

UPDATE dbo.IsolationTest
SET Balance = 500
WHERE Id = 1;

-- Keep the transaction open.
-- Do not COMMIT yet.
-- Now switch to Session 2.


-- Session 2

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

SELECT *
FROM dbo.IsolationTest
WHERE Id = 1;


-- ============================================================
-- 2. Non-Repeatable Read
-- ============================================================

-- Session 1

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

BEGIN TRANSACTION;

SELECT Balance
FROM dbo.IsolationTest
WHERE Id = 1;

-- Keep the transaction open.
-- Now switch to Session 2.


-- Session 2

UPDATE dbo.IsolationTest
SET Balance = 750
WHERE Id = 1;

COMMIT;


-- Session 1 — Second Read

SELECT Balance
FROM dbo.IsolationTest
WHERE Id = 1;

COMMIT;


-- ============================================================
-- 3. Phantom Read
-- ============================================================

-- Session 1

SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;

BEGIN TRANSACTION;

SELECT *
FROM dbo.IsolationTest
WHERE Balance >= 1000;

-- Keep the transaction open.
-- Now switch to Session 2.


-- Session 2

INSERT INTO dbo.IsolationTest (Id, Name, Balance)
VALUES (4, 'David', 1500);

COMMIT;


-- Session 1 — Second Read

SELECT *
FROM dbo.IsolationTest
WHERE Balance >= 1000;

COMMIT;


-- ============================================================
-- ISOLATION LEVEL SUMMARY
-- ============================================================

-- Dirty Read          → READ COMMITTED
-- Non-Repeatable Read → REPEATABLE READ
-- Phantom Read        → SERIALIZABLE