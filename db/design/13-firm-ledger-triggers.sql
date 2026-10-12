-- MatterForge firm database: LEDGER TRIGGERS. DESIGN ARTIFACT, NOT A MIGRATION.
-- Applies after 12-firm-schema-future.sql.
--
-- Validated finding (MySQL 8.0.36, binary logging ON by default): CREATE TRIGGER by an account
-- without SUPER fails with ERROR 1419 unless log_bin_trust_function_creators = 1. The design
-- therefore creates triggers in a privileged provisioning step that sets DEFINER to the firm's
-- migration account (run-validation.ps1 shows both the failure and this path). Choosing between
-- that step and the server parameter is a hosting decision; see unresolved decisions.

-- Ledger invariants a row CHECK cannot express. Locking reads (FOR SHARE / FOR UPDATE) make
-- the checks see the latest committed rows rather than a REPEATABLE READ snapshot.
DELIMITER $$
CREATE TRIGGER TR_journal_lines_bi BEFORE INSERT ON journal_lines FOR EACH ROW
BEGIN
    DECLARE entry_status varchar(16);
    SELECT Status INTO entry_status FROM journal_entries WHERE Id = NEW.JournalEntryId FOR UPDATE;
    IF entry_status IS NULL OR entry_status <> 'Draft' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'journal_entry_not_draft';
    END IF;
END$$

CREATE TRIGGER TR_journal_lines_bu BEFORE UPDATE ON journal_lines FOR EACH ROW
BEGIN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'journal_line_immutable';
END$$

CREATE TRIGGER TR_journal_lines_bd BEFORE DELETE ON journal_lines FOR EACH ROW
BEGIN
    DECLARE entry_status varchar(16);
    SELECT Status INTO entry_status FROM journal_entries WHERE Id = OLD.JournalEntryId FOR UPDATE;
    IF entry_status <> 'Draft' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'journal_entry_not_draft';
    END IF;
END$$

CREATE TRIGGER TR_journal_entries_bu BEFORE UPDATE ON journal_entries FOR EACH ROW
BEGIN
    DECLARE line_count int;
    DECLARE currency_count int;
    DECLARE difference decimal(21,2);
    IF OLD.Status = 'Posted' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'journal_entry_posted_immutable';
    END IF;
    IF NEW.Status = 'Posted' THEN
        -- Single currency per entry while CurrencyCode is restricted to USD.
        SELECT COUNT(*), COUNT(DISTINCT CurrencyCode), COALESCE(SUM(Debit) - SUM(Credit), 0)
          INTO line_count, currency_count, difference
          FROM journal_lines WHERE JournalEntryId = NEW.Id FOR SHARE;
        IF line_count < 2 THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'journal_entry_too_few_lines';
        END IF;
        IF currency_count <> 1 OR difference <> 0 THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'journal_entry_unbalanced';
        END IF;
    END IF;
END$$

CREATE TRIGGER TR_journal_entries_bd BEFORE DELETE ON journal_entries FOR EACH ROW
BEGIN
    IF OLD.Status = 'Posted' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'journal_entry_posted_immutable';
    END IF;
END$$
DELIMITER ;
