<#
.SYNOPSIS
    Validates the MatterForge design SQL on a DISPOSABLE MySQL container.

.DESCRIPTION
    Starts a new, uniquely named mysql container with a random root password that is never
    printed, applies the control-plane and firm design DDL, creates two synthetic firm
    databases with restricted users, runs privilege, constraint, concurrency, and query-plan
    checks, writes a log under validation/, and removes the container.

    It never connects to any other server. It does not read application configuration or
    connection strings. Requires Docker.
#>
[CmdletBinding()]
param(
    [string]$Image = 'mysql:8.0.36',
    [switch]$KeepContainer
)

$ErrorActionPreference = 'Stop'
$OutputEncoding = New-Object System.Text.UTF8Encoding($false)
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$outDir = Join-Path $here 'validation'
New-Item -ItemType Directory -Force $outDir | Out-Null
$log = Join-Path $outDir 'mysql-8.0.36-validation.log'
$container = 'mf-design-validation-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$secrets = @{}

function New-Secret {
    -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object { [char]$_ })
}

function Write-Log([string]$Text) {
    Add-Content -Path $log -Value $Text -Encoding UTF8
    Write-Host $Text
}

function Invoke-MySql {
    param([string]$Sql, [string]$User = 'root', [string]$Database = '', [switch]$Plain)
    $env:MYSQL_PWD = $secrets[$User]
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $arguments = @('exec', '-i', '-e', 'MYSQL_PWD', $container, 'mysql', '--force',
            '--default-character-set=utf8mb4', '-h', '127.0.0.1', '-u', $User)
        if ($Plain) { $arguments += @('-s', '-r') } else { $arguments += '--table' }
        if ($Database) { $arguments += $Database }
        $Sql | & docker @arguments 2>&1 | ForEach-Object { "$_" } |
            Where-Object { $_ -notmatch 'Using a password on the command line' }
    }
    finally {
        $ErrorActionPreference = $previous
        Remove-Item Env:MYSQL_PWD -ErrorAction SilentlyContinue
    }
}

function Invoke-Logged {
    param([string]$Title, [string]$Sql, [string]$User = 'root', [string]$Database = '', [switch]$Plain)
    Write-Log ''
    Write-Log "### $Title (as $User$(if ($Database) { " on $Database" }))"
    Invoke-MySql -Sql $Sql -User $User -Database $Database -Plain:$Plain | ForEach-Object { Write-Log $_ }
}

function Start-MySqlSession {
    param([string]$Sql, [string]$User, [string]$Database)
    Start-Job -ArgumentList $container, $User, $Database, $Sql, $secrets[$User] -ScriptBlock {
        param($c, $u, $db, $sql, $pw)
        $env:MYSQL_PWD = $pw
        $ErrorActionPreference = 'Continue'
        $sql | & docker exec -i -e MYSQL_PWD $c mysql --force -s -r -h 127.0.0.1 -u $u $db 2>&1 |
            ForEach-Object { "$_" } | Where-Object { $_ -notmatch 'Using a password' }
    }
}

function Get-FileSql([string]$Name) { Get-Content -Raw -Path (Join-Path $here $Name) }

Set-Content -Path $log -Value "MatterForge design validation log (synthetic data only)" -Encoding UTF8
Write-Log "Run started (UTC): $([DateTime]::UtcNow.ToString('yyyy-MM-dd HH:mm:ss'))"
Write-Log "Image: $Image; container: $container (disposable, removed at end unless -KeepContainer)"
Write-Log "Docker: $(docker version --format '{{.Server.Version}} (API {{.Server.APIVersion}})')"

$secrets['root'] = New-Secret
$env:MYSQL_ROOT_PASSWORD = $secrets['root']
try {
    docker run -d --name $container -e MYSQL_ROOT_PASSWORD $Image | Out-Null
}
finally {
    Remove-Item Env:MYSQL_ROOT_PASSWORD -ErrorAction SilentlyContinue
}

try {
    $deadline = (Get-Date).AddMinutes(4)
    do {
        Start-Sleep -Seconds 3
        $ErrorActionPreference = 'Continue'
        $ready = (docker logs $container 2>&1 | ForEach-Object { "$_" } | Out-String) -match 'ready for connections\. Version: .*port: 3306'
        $ErrorActionPreference = 'Stop'
    } until ($ready -or (Get-Date) -gt $deadline)
    if (-not $ready) { throw 'MySQL container did not become ready.' }
    Start-Sleep -Seconds 2

    $suffix = { [guid]::NewGuid().ToString('N') }
    $firms = @(
        [ordered]@{ Label = 'A'; Scale = '1.0'; FirmId = [guid]::NewGuid().ToString(); Db = 'mf_f_' + (& $suffix) },
        [ordered]@{ Label = 'B'; Scale = '0.1'; FirmId = [guid]::NewGuid().ToString(); Db = 'mf_f_' + (& $suffix) }
    )
    foreach ($f in $firms) {
        $f.Mig = 'mf_mig_' + $f.Label.ToLower()
        $f.Rt = 'mf_rt_' + $f.Label.ToLower()
        $secrets[$f.Mig] = New-Secret
        $secrets[$f.Rt] = New-Secret
    }
    $secrets['mf_cp_rt'] = New-Secret
    $secrets['zz_dbwide'] = New-Secret
    $secrets['zz_pooled'] = New-Secret
    $secrets['zz_capped'] = New-Secret

    Invoke-Logged 'Engine behavior checks (60-engine-behavior-checks.sql)' `
        ("CREATE DATABASE mf_engine_checks; USE mf_engine_checks;`n" + (Get-FileSql '60-engine-behavior-checks.sql'))

    # ---------------------------------------------------------------- control plane
    Invoke-MySql -Sql "CREATE DATABASE mf_control CHARACTER SET utf8mb4;" | Out-Null
    Invoke-Logged 'Apply 01-control-plane.sql' (Get-FileSql '01-control-plane.sql') -Database 'mf_control'
    Invoke-Logged 'Control-plane tables created' "SELECT COUNT(*) AS tables FROM information_schema.TABLES WHERE TABLE_SCHEMA='mf_control';"
    Invoke-MySql -Sql @"
CREATE USER 'mf_cp_rt'@'%' IDENTIFIED BY '$($secrets['mf_cp_rt'])';
GRANT SELECT, INSERT, UPDATE ON mf_control.* TO 'mf_cp_rt'@'%';
"@ | Out-Null

    # ---------------------------------------------------------------- firm databases
    $appendOnly = @('audit_events', 'custody_events', 'document_versions', 'matter_stage_history',
        'intake_stage_history', 'settlement_approvals', 'trust_disbursement_approvals', 'outbox_events_archive')
    $selectOnly = @('firm_identity', '__EFMigrationsHistory')
    $insertDelete = @('journal_lines')

    foreach ($f in $firms) {
        $db = $f.Db
        Invoke-MySql -Sql @"
CREATE DATABASE $db CHARACTER SET utf8mb4;
CREATE USER '$($f.Mig)'@'%' IDENTIFIED BY '$($secrets[$f.Mig])';
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES, TRIGGER,
      CREATE VIEW, SHOW VIEW, LOCK TABLES, CREATE TEMPORARY TABLES ON ``$db``.* TO '$($f.Mig)'@'%';
"@ | Out-Null
        $sw = [Diagnostics.Stopwatch]::StartNew()
        Invoke-Logged "Firm $($f.Label): apply 10 (generated EF schema) as migration user" (Get-FileSql '10-firm-current-schema.generated.sql') -User $f.Mig -Database $db
        Invoke-Logged "Firm $($f.Label): apply 11 (required additions) as migration user" (Get-FileSql '11-firm-schema-required.sql') -User $f.Mig -Database $db
        Invoke-Logged "Firm $($f.Label): apply 12 (future structures, includes triggers) as migration user" (Get-FileSql '12-firm-schema-future.sql') -User $f.Mig -Database $db
        $triggerSql = Get-FileSql '13-firm-ledger-triggers.sql'
        Invoke-Logged "Firm $($f.Label): apply 13 (ledger triggers) as migration user - EXPECT 1419 while binary logging is on" $triggerSql -User $f.Mig -Database $db
        $definer = "CREATE DEFINER = '$($f.Mig)'@'%' TRIGGER"
        Invoke-Logged "Firm $($f.Label): apply 13 through privileged provisioning step with DEFINER = migration user" ($triggerSql -replace 'CREATE TRIGGER', $definer) -Database $db
        Write-Log "Schema apply elapsed: $([math]::Round($sw.Elapsed.TotalSeconds,1)) s"
        Invoke-Logged "Firm $($f.Label): object counts" @"
SELECT (SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA='$db') AS tables,
       (SELECT COUNT(*) FROM information_schema.TRIGGERS WHERE TRIGGER_SCHEMA='$db') AS triggers,
       (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA='$db' AND CONSTRAINT_TYPE='FOREIGN KEY') AS foreign_keys,
       (SELECT COUNT(*) FROM information_schema.CHECK_CONSTRAINTS WHERE CONSTRAINT_SCHEMA='$db') AS checks;
"@
        Invoke-MySql -User $f.Mig -Database $db -Sql @"
INSERT INTO firm_identity (Singleton, FirmId, FirmSlug, PlacementRoutingVersion, ProvisionedAtUtc)
VALUES (1, '$($f.FirmId)', 'synthetic-firm-$($f.Label.ToLower())', 1, UTC_TIMESTAMP(6));
"@ | ForEach-Object { Write-Log $_ }

        $sw.Restart()
        Invoke-Logged "Firm $($f.Label): synthetic data at scale $($f.Scale)" `
            ("SET @scale = $($f.Scale), @firm = '$($f.Label)';`n" + (Get-FileSql '40-synthetic-data.sql')) -Database $db
        Write-Log "Synthetic load elapsed: $([math]::Round($sw.Elapsed.TotalSeconds,1)) s"

        # Table-level runtime grants generated from the live table list (no database-wide grant).
        $tables = Invoke-MySql -Plain -Sql "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA='$db' ORDER BY TABLE_NAME;"
        $grants = New-Object System.Collections.Generic.List[string]
        $grants.Add("CREATE USER '$($f.Rt)'@'%' IDENTIFIED BY '$($secrets[$f.Rt])' WITH MAX_USER_CONNECTIONS 20;")
        foreach ($t in $tables) {
            $priv = if ($selectOnly -contains $t) { 'SELECT' }
                    elseif ($appendOnly -contains $t) { 'SELECT, INSERT' }
                    elseif ($insertDelete -contains $t) { 'SELECT, INSERT, DELETE' }
                    else { 'SELECT, INSERT, UPDATE, DELETE' }
            $grants.Add("GRANT $priv ON ``$db``.``$t`` TO '$($f.Rt)'@'%';")
        }
        Invoke-MySql -Sql ($grants -join "`n") | ForEach-Object { Write-Log $_ }
        Write-Log "Runtime user for firm $($f.Label): $($tables.Count) table-level grants, MAX_USER_CONNECTIONS 20"
    }
    $A = $firms[0]; $B = $firms[1]

    Invoke-Logged 'ANALYZE firm A tables' ("ANALYZE TABLE matters, matter_tasks, matter_access_grants, matter_assignments, deadlines, matter_documents, document_versions, messages, invoices, payment_allocations, journal_lines, journal_entries, trust_subledger_balances, intake_records, consultation_requests, outbox_events, webhook_receipts;") -Database $A.Db

    # ---------------------------------------------------------------- privilege checks
    Invoke-Logged 'G1 EXPECT 1142: firm A runtime reads firm B table' "SELECT COUNT(*) FROM ``$($B.Db)``.matters;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G2 EXPECT 1044: firm A runtime switches to firm B database' "USE ``$($B.Db)``;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G3 firm A runtime sees only its own firm database' "SHOW DATABASES;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G4 EXPECT 1142: runtime CREATE TABLE' "CREATE TABLE zz_x (i int);" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G5 EXPECT 1142: runtime DROP TABLE' "DROP TABLE matters;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G6 EXPECT 1142: runtime ALTER TABLE' "ALTER TABLE matters ADD COLUMN x int;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G7 EXPECT 1142 x2: runtime UPDATE and DELETE on append-only audit_events' "UPDATE audit_events SET Action='x' LIMIT 1; DELETE FROM audit_events LIMIT 1;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G8 EXPECT 1142 x2: runtime writes firm_identity' "UPDATE firm_identity SET FirmSlug='x'; INSERT INTO firm_identity (Singleton, FirmId, FirmSlug, PlacementRoutingVersion, ProvisionedAtUtc) VALUES (1,'x','x',1,NOW());" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G9 EXPECT 1142: runtime reads mysql.user' "SELECT COUNT(*) FROM mysql.user;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G10 EXPECT 1227: runtime changes a global variable' "SET GLOBAL max_connections = 1;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G11 positive control: runtime appends audit, updates a matter' "INSERT INTO audit_events (Id, Action, ObjectType, ObjectId, ActorId, ActorDisplayName, CorrelationId, MetadataJson, OccurredAtUtc) VALUES (UUID(),'check','matter','x','u-s-001','Synthetic','c','{}',UTC_TIMESTAMP(6)); UPDATE matters SET Title = CONCAT(Title, ' (checked)') WHERE MatterNumber = 'MAT-000001'; SELECT ROW_COUNT() AS rows_updated;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G12 firm identity row read by runtime A' "SELECT FirmId = '$($A.FirmId)' AS matches_firm_a, FirmSlug FROM firm_identity;" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G13 overlapping local ids: same matter id in A and B holds different rows' "SELECT Id, Title FROM matters WHERE Id = '00000001-0000-4000-8000-000000000001';" -User $A.Rt -Database $A.Db
    Invoke-Logged 'G13b same id in firm B (read by B runtime)' "SELECT Id, Title FROM matters WHERE Id = '00000001-0000-4000-8000-000000000001';" -User $B.Rt -Database $B.Db
    Invoke-Logged 'G14 EXPECT 1142 and 1044: firm A migration user reads B and creates a database' "SELECT COUNT(*) FROM ``$($B.Db)``.matters; CREATE DATABASE mf_f_00000000000000000000000000000000;" -User $A.Mig -Database $A.Db
    Invoke-Logged 'G15 EXPECT 1142: control-plane runtime reads a firm database' "SELECT COUNT(*) FROM ``$($A.Db)``.matters;" -User 'mf_cp_rt' -Database 'mf_control'
    Invoke-Logged 'G16 EXPECT 1141 or 1147: table-level REVOKE cannot carve out of a database-level grant' @"
CREATE USER 'zz_dbwide'@'%' IDENTIFIED BY '$($secrets['zz_dbwide'])';
GRANT SELECT, INSERT, UPDATE, DELETE ON ``$($A.Db)``.* TO 'zz_dbwide'@'%';
REVOKE UPDATE ON ``$($A.Db)``.audit_events FROM 'zz_dbwide'@'%';
"@
    Invoke-Logged 'G16b database-level user can therefore still UPDATE audit_events (expect ok = 1)' "UPDATE audit_events SET Action = Action ORDER BY OccurredAtUtc LIMIT 1; SELECT 1 AS ok;" -User 'zz_dbwide' -Database $A.Db

    # ---------------------------------------------------------------- pooled alternative
    Invoke-MySql -Sql "CREATE DATABASE mf_pooled_check CHARACTER SET utf8mb4;" | Out-Null
    Invoke-Logged 'Apply 20-pooled-alternative.sql' (Get-FileSql '20-pooled-alternative.sql') -Database 'mf_pooled_check'
    $fa = '0a000000-0000-4000-8000-00000000000a'; $fb = '0b000000-0000-4000-8000-00000000000b'
    $m1 = '00000001-0000-4000-8000-000000000001'; $m2 = '00000001-0000-4000-8000-000000000002'
    Invoke-Logged 'P1 pooled: overlapping matter id in two firms, cross-firm reference (EXPECT 1452), same-firm reference ok' @"
INSERT INTO pooled_firms VALUES ('$fa','pooled-a'), ('$fb','pooled-b');
INSERT INTO pooled_matters VALUES ('$fa','$m1','A-1','A one','Active','u-s-001',NOW(6)),
                                  ('$fb','$m1','B-1','B one','Active','u-s-001',NOW(6)),
                                  ('$fb','$m2','B-2','B two','Active','u-s-001',NOW(6));
INSERT INTO pooled_matter_tasks VALUES ('$fa', UUID(), '$m2', 'cross-firm task', 'Open', NULL);
INSERT INTO pooled_matter_tasks VALUES ('$fa', '00000007-0000-4000-8000-000000000001', '$m1', 'same-firm task', 'Open', NULL);
SELECT COUNT(*) AS tasks_for_firm_a FROM pooled_matter_tasks WHERE FirmId = '$fa';
"@ -Database 'mf_pooled_check'
    Invoke-Logged 'P2 pooled: a query that omits FirmId returns both firms (FKs do not authorize reads)' "SELECT FirmId, Title FROM pooled_matters WHERE Id = '$m1';" -Database 'mf_pooled_check'
    Invoke-Logged 'P3 pooled: updating FirmId silently re-parents a task to firm B when local ids overlap (expect rows_moved = 1)' @"
UPDATE pooled_matter_tasks SET FirmId = '$fb' WHERE Id = '00000007-0000-4000-8000-000000000001';
SELECT ROW_COUNT() AS rows_moved;
UPDATE pooled_matter_tasks SET FirmId = '$fa' WHERE Id = '00000007-0000-4000-8000-000000000001';
"@ -Database 'mf_pooled_check'
    Invoke-Logged 'P4 pooled mitigation: column-level UPDATE grant excludes FirmId (EXPECT 1143)' @"
CREATE USER 'zz_pooled'@'%' IDENTIFIED BY '$($secrets['zz_pooled'])';
GRANT SELECT ON mf_pooled_check.pooled_matter_tasks TO 'zz_pooled'@'%';
GRANT UPDATE (Title, Status, DueAtUtc) ON mf_pooled_check.pooled_matter_tasks TO 'zz_pooled'@'%';
"@
    Invoke-Logged 'P4b zz_pooled updates FirmId' "UPDATE pooled_matter_tasks SET FirmId = '$fb' WHERE Id = '00000007-0000-4000-8000-000000000001';" -User 'zz_pooled' -Database 'mf_pooled_check'

    # ---------------------------------------------------------------- money integrity
    Invoke-Logged 'Money and approval integrity checks (70-money-integrity-checks.sql)' (Get-FileSql '70-money-integrity-checks.sql') -User $B.Rt -Database $B.Db

    # ---------------------------------------------------------------- concurrency
    Write-Log ''
    Write-Log '### R1 race: session 1 adds an unbalancing line and holds; session 2 posts the entry'
    Invoke-MySql -User $B.Rt -Database $B.Db -Sql @"
INSERT INTO journal_entries (Id, EntryNumber, EffectiveDate, Status, SourceType, IdempotencyKey, CreatedByActorId, CreatedAtUtc)
VALUES ('aaaaaaaa-0000-4000-8000-0000000000r1', 9100001, '2026-10-12', 'Draft', 'Race', 'race-1', 'u-s-031', UTC_TIMESTAMP(6));
INSERT INTO journal_lines (JournalEntryId, LineNumber, LedgerAccountId, Debit, Credit) VALUES
 ('aaaaaaaa-0000-4000-8000-0000000000r1', 1, '00000014-0000-4000-8000-000000000001', 50.00, 0),
 ('aaaaaaaa-0000-4000-8000-0000000000r1', 2, '00000014-0000-4000-8000-000000000005', 0, 50.00);
"@ | ForEach-Object { Write-Log $_ }
    $s1 = Start-MySqlSession -User $B.Rt -Database $B.Db -Sql @"
START TRANSACTION;
INSERT INTO journal_lines (JournalEntryId, LineNumber, LedgerAccountId, Debit, Credit)
VALUES ('aaaaaaaa-0000-4000-8000-0000000000r1', 3, '00000014-0000-4000-8000-000000000001', 5.00, 0);
SELECT SLEEP(4);
COMMIT;
SELECT 'session1 committed extra line';
"@
    Start-Sleep -Seconds 2
    $t0 = Get-Date
    Invoke-MySql -User $B.Rt -Database $B.Db -Plain -Sql @"
START TRANSACTION;
UPDATE journal_entries SET Status = 'Posted', PostedAtUtc = UTC_TIMESTAMP(6), PostedByActorId = 'u-s-001'
WHERE Id = 'aaaaaaaa-0000-4000-8000-0000000000r1';
COMMIT;
"@ | ForEach-Object { Write-Log "session2: $_" }
    Write-Log "session2 waited approximately $([math]::Round(((Get-Date) - $t0).TotalSeconds,1)) s"
    Receive-Job -Job $s1 -Wait -AutoRemoveJob | ForEach-Object { Write-Log "session1: $_" }
    Invoke-Logged 'R1 result (expect Status Draft, 3 lines)' "SELECT e.Status, COUNT(l.Id) AS line_count FROM journal_entries e JOIN journal_lines l ON l.JournalEntryId = e.Id WHERE e.Id = 'aaaaaaaa-0000-4000-8000-0000000000r1' GROUP BY e.Status;" -User $B.Rt -Database $B.Db

    Write-Log ''
    Write-Log '### R2 race: two concurrent withdrawals that together overdraw one trust subledger'
    $key = "TrustAccountId = '00000014-0000-4000-8000-000000000002' AND MatterRecordId = '00000001-0000-4000-8000-000000000004' AND ClientPartyId = '00000002-0000-4000-8000-000000000004'"
    $before = Invoke-MySql -Plain -User $B.Rt -Database $B.Db -Sql "SELECT Balance FROM trust_subledger_balances WHERE $key;"
    Write-Log "balance before: $before"
    $withdraw = [math]::Round([double]$before * 0.6, 2)
    $s2 = Start-MySqlSession -User $B.Rt -Database $B.Db -Sql @"
START TRANSACTION;
SELECT Balance FROM trust_subledger_balances WHERE $key FOR UPDATE;
UPDATE trust_subledger_balances SET Balance = Balance - $withdraw, RowVersion = RowVersion + 1 WHERE $key;
SELECT SLEEP(4);
COMMIT;
SELECT 'session1 committed withdrawal';
"@
    Start-Sleep -Seconds 2
    Invoke-MySql -User $B.Rt -Database $B.Db -Plain -Sql @"
START TRANSACTION;
SELECT Balance FROM trust_subledger_balances WHERE $key FOR UPDATE;
UPDATE trust_subledger_balances SET Balance = Balance - $withdraw, RowVersion = RowVersion + 1 WHERE $key;
COMMIT;
"@ | ForEach-Object { Write-Log "session2: $_" }
    Receive-Job -Job $s2 -Wait -AutoRemoveJob | ForEach-Object { Write-Log "session1: $_" }
    Invoke-Logged "R2 result (expect balance = before - $withdraw)" "SELECT Balance FROM trust_subledger_balances WHERE $key;" -User $B.Rt -Database $B.Db

    Write-Log ''
    Write-Log '### R3 outbox claim with FOR UPDATE SKIP LOCKED: two dispatchers must claim disjoint rows'
    $claim = "START TRANSACTION; SELECT GROUP_CONCAT(Id ORDER BY Id) FROM (SELECT Id FROM outbox_events WHERE Status = 'Pending' ORDER BY AvailableAtUtc, Id LIMIT 5 FOR UPDATE SKIP LOCKED) c;"
    $s3 = Start-MySqlSession -User $B.Rt -Database $B.Db -Sql ($claim + " SELECT SLEEP(4); COMMIT;")
    Start-Sleep -Seconds 2
    Invoke-MySql -User $B.Rt -Database $B.Db -Plain -Sql ($claim + " COMMIT;") | ForEach-Object { Write-Log "dispatcher2 claimed: $_" }
    Receive-Job -Job $s3 -Wait -AutoRemoveJob | Where-Object { $_ -ne '0' } | ForEach-Object { Write-Log "dispatcher1 claimed: $_" }

    Write-Log ''
    Write-Log '### R4 per-user connection cap (MAX_USER_CONNECTIONS 1): second concurrent session EXPECT 1226'
    Invoke-MySql -Sql "CREATE USER 'zz_capped'@'%' IDENTIFIED BY '$($secrets['zz_capped'])' WITH MAX_USER_CONNECTIONS 1; GRANT SELECT ON ``$($B.Db)``.matters TO 'zz_capped'@'%';" | Out-Null
    $s4 = Start-MySqlSession -User 'zz_capped' -Database $B.Db -Sql "SELECT SLEEP(4);"
    Start-Sleep -Seconds 2
    Invoke-MySql -User 'zz_capped' -Database $B.Db -Plain -Sql "SELECT 'second session connected';" | ForEach-Object { Write-Log "session2: $_" }
    Receive-Job -Job $s4 -Wait -AutoRemoveJob | Out-Null

    # ---------------------------------------------------------------- data shape and plans
    Invoke-Logged 'Firm A distributions' @"
SELECT 'matters' AS t, Status AS v, COUNT(*) AS n FROM matters GROUP BY Status
UNION ALL SELECT 'matter_tasks', Status, COUNT(*) FROM matter_tasks GROUP BY Status
UNION ALL SELECT 'deadlines', Status, COUNT(*) FROM deadlines GROUP BY Status
UNION ALL SELECT 'invoices', Status, COUNT(*) FROM invoices GROUP BY Status
UNION ALL SELECT 'outbox_events', Status, COUNT(*) FROM outbox_events GROUP BY Status
UNION ALL SELECT 'matters for u-s-003 (responsible)', 'all', COUNT(*) FROM matters WHERE ResponsibleAttorneyActorId = 'u-s-003'
UNION ALL SELECT 'messages in hot conversation', 'n', COUNT(*) FROM messages WHERE ConversationId = '0000000a-0000-4000-8000-000000000001';
"@ -User $A.Rt -Database $A.Db
    Invoke-Logged 'Firm A largest tables (information_schema estimates after ANALYZE)' @"
SELECT TABLE_NAME, TABLE_ROWS AS est_rows, ROUND(DATA_LENGTH/1048576,1) AS data_mb, ROUND(INDEX_LENGTH/1048576,1) AS index_mb
FROM information_schema.TABLES WHERE TABLE_SCHEMA = '$($A.Db)' ORDER BY DATA_LENGTH + INDEX_LENGTH DESC LIMIT 16;
SELECT ROUND(SUM(DATA_LENGTH + INDEX_LENGTH)/1048576,1) AS total_mb FROM information_schema.TABLES WHERE TABLE_SCHEMA = '$($A.Db)';
"@
    Invoke-Logged 'Representative query plans on firm A (50-representative-queries.sql)' (Get-FileSql '50-representative-queries.sql') -User $A.Rt -Database $A.Db -Plain

    Write-Log ''
    Write-Log "Run finished (UTC): $([DateTime]::UtcNow.ToString('yyyy-MM-dd HH:mm:ss'))"
}
finally {
    if (-not $KeepContainer) {
        $ErrorActionPreference = 'Continue'
        docker rm -f $container 2>&1 | Out-Null
        Write-Log "Container $container removed."
    }
}
