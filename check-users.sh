#!/bin/bash
sqlite3 IskoWalk.db << 'SQL'
.headers on
.mode column
SELECT * FROM Users;
SQL
