#!/bin/bash
sqlite3 IskoWalk.db << 'SQL'
INSERT OR REPLACE INTO Users (UserId, Username, Email, FullName, PasswordHash, CreatedAt)
VALUES (
    'juan.dc',
    'juan.dc',
    'juan.dc@iskolarngbayan.pup.edu.ph',
    'Juan Dela Cruz',
    'dummy-hash-for-testing',
    datetime('now')
);

.headers on
.mode column
SELECT * FROM Users WHERE UserId = 'juan.dc';
SQL
