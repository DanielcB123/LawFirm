-- Engine behavior checks relevant to the tenancy and migration design. Run as an
-- administrative user in a scratch database on a DISPOSABLE server, with mysql --force so
-- that expected errors are printed and execution continues.

SELECT VERSION() AS version, @@transaction_isolation AS isolation, @@log_bin AS log_bin,
       @@log_bin_trust_function_creators AS trust_function_creators, @@max_connections AS max_connections,
       @@innodb_lock_wait_timeout AS lock_wait_timeout, @@default_storage_engine AS engine;

-- ---------------------------------------------------------------- partitioning and FKs
SELECT 'E1 EXPECT ERROR 1506: partitioned InnoDB table that declares a foreign key' AS check_name;
CREATE TABLE e_parent (Id int PRIMARY KEY);
CREATE TABLE e_child_partitioned (
    Id int NOT NULL, ParentId int NOT NULL, CreatedOn date NOT NULL,
    PRIMARY KEY (Id, CreatedOn),
    FOREIGN KEY (ParentId) REFERENCES e_parent (Id)
) PARTITION BY RANGE COLUMNS (CreatedOn) (
    PARTITION p2025 VALUES LESS THAN ('2026-01-01'),
    PARTITION pmax VALUES LESS THAN (MAXVALUE));

SELECT 'E2 EXPECT ERROR 1506: foreign key that references a partitioned table' AS check_name;
CREATE TABLE e_partitioned_parent (
    Id int NOT NULL, CreatedOn date NOT NULL, PRIMARY KEY (Id, CreatedOn)
) PARTITION BY RANGE COLUMNS (CreatedOn) (
    PARTITION p2025 VALUES LESS THAN ('2026-01-01'),
    PARTITION pmax VALUES LESS THAN (MAXVALUE));
CREATE TABLE e_child_of_partitioned (
    Id int PRIMARY KEY, ParentId int NOT NULL, ParentCreatedOn date NOT NULL,
    FOREIGN KEY (ParentId, ParentCreatedOn) REFERENCES e_partitioned_parent (Id, CreatedOn));

SELECT 'E3 EXPECT ERROR 1503: every unique key must include the partitioning column' AS check_name;
CREATE TABLE e_unique_partitioned (
    Id int NOT NULL, ExternalRef varchar(64) NOT NULL, CreatedOn date NOT NULL,
    PRIMARY KEY (Id, CreatedOn), UNIQUE KEY (ExternalRef)
) PARTITION BY RANGE COLUMNS (CreatedOn) (PARTITION pmax VALUES LESS THAN (MAXVALUE));

-- ---------------------------------------------------------------- DDL implicit commit
SELECT 'E4 DDL implicitly commits an open transaction: expect committed_rows = 1' AS check_name;
CREATE TABLE e_ic (Id int PRIMARY KEY);
START TRANSACTION;
INSERT INTO e_ic VALUES (1);
CREATE TABLE e_ic_other (Id int);
ROLLBACK;
SELECT COUNT(*) AS committed_rows FROM e_ic;

SELECT 'E5 multi-statement migration failing midway leaves earlier DDL applied: expect step1 exists' AS check_name;
START TRANSACTION;
CREATE TABLE e_mig_step1 (Id int PRIMARY KEY);
ALTER TABLE e_mig_missing ADD COLUMN x int;
ROLLBACK;
SELECT COUNT(*) AS step1_table_exists FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'e_mig_step1';

-- ---------------------------------------------------------------- online DDL algorithms
SELECT 'E6 ADD COLUMN with ALGORITHM=INSTANT: expect success' AS check_name;
ALTER TABLE e_ic ADD COLUMN Note varchar(100) NULL, ALGORITHM = INSTANT;

SELECT 'E7 EXPECT ERROR 1845/1846: ADD INDEX cannot be INSTANT' AS check_name;
ALTER TABLE e_ic ADD INDEX IX_e_ic_Note (Note), ALGORITHM = INSTANT;

SELECT 'E8 ADD INDEX with ALGORITHM=INPLACE, LOCK=NONE: expect success' AS check_name;
ALTER TABLE e_ic ADD INDEX IX_e_ic_Note (Note), ALGORITHM = INPLACE, LOCK = NONE;

SELECT 'E9 EXPECT ERROR 1846: changing a column type cannot be INPLACE/LOCK=NONE' AS check_name;
ALTER TABLE e_ic MODIFY COLUMN Note varchar(50) NULL, ALGORITHM = INPLACE, LOCK = NONE;

-- ---------------------------------------------------------------- collation of id columns
SELECT 'E10 EXPECT ERROR 3780: FK between char(36) ascii_general_ci and char(36) utf8mb4' AS check_name;
CREATE TABLE e_ascii_parent (Id char(36) COLLATE ascii_general_ci PRIMARY KEY);
CREATE TABLE e_utf8_child (Id int PRIMARY KEY, ParentId char(36) CHARACTER SET utf8mb4,
    FOREIGN KEY (ParentId) REFERENCES e_ascii_parent (Id));

-- ---------------------------------------------------------------- CHECK and generated columns
SELECT 'E11 EXPECT ERROR 3819: CHECK constraint enforced (8.0.16+)' AS check_name;
CREATE TABLE e_check (Amount decimal(19,2) NOT NULL, CONSTRAINT CK_e_check CHECK (Amount >= 0));
INSERT INTO e_check VALUES (-0.01);

SELECT 'E12 unique index over a nullable STORED generated column allows many inactive rows: expect 3 rows' AS check_name;
CREATE TABLE e_active (K int NOT NULL, EndedAt datetime NULL,
    ActiveMarker tinyint AS (CASE WHEN EndedAt IS NULL THEN 1 ELSE NULL END) STORED,
    UNIQUE KEY (K, ActiveMarker));
INSERT INTO e_active (K, EndedAt) VALUES (1, NOW()), (1, NOW()), (1, NULL);
SELECT COUNT(*) AS rows_for_k FROM e_active;
SELECT 'E12b EXPECT ERROR 1062: second active row for the same key' AS check_name;
INSERT INTO e_active (K, EndedAt) VALUES (1, NULL);
