[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$containerName = "concertable-customer-migrations-$PID-$([Guid]::NewGuid().ToString('N').Substring(0, 8))"
$password = 'Customer_migrations_123!'
$connectionVariable = 'ConnectionStrings__CustomerDb'
$environmentVariable = 'DOTNET_ENVIRONMENT'
$previousConnection = [Environment]::GetEnvironmentVariable($connectionVariable, 'Process')
$previousEnvironment = [Environment]::GetEnvironmentVariable($environmentVariable, 'Process')
$containerStarted = $false

# The User module persists a geography Point, so the job needs PostGIS present before it migrates.
$image = 'postgis/postgis:17-3.5'

try {
    & docker run --detach --rm --name $containerName `
        --env "POSTGRES_PASSWORD=$password" `
        --env 'POSTGRES_DB=CustomerDb' `
        --publish '127.0.0.1::5432' `
        $image | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'Failed to start the temporary PostgreSQL container.'
    }
    $containerStarted = $true

    $portOutput = & docker port $containerName '5432/tcp'
    if ($LASTEXITCODE -ne 0 -or $portOutput -notmatch ':(?<port>\d+)\s*$') {
        throw "Could not resolve the temporary PostgreSQL port: $portOutput"
    }
    $port = $Matches.port

    $ready = $false
    for ($attempt = 1; $attempt -le 180; $attempt++) {
        # The entrypoint's bootstrap server listens on the unix socket only, so a socket probe reports
        # ready while the published port the job connects through still has nothing behind it.
        & docker exec $containerName pg_isready --host 127.0.0.1 --port 5432 --username postgres --dbname CustomerDb 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            break
        }

        Start-Sleep -Seconds 1
    }

    if (-not $ready) {
        $containerLogs = & docker logs $containerName 2>&1
        throw "Temporary PostgreSQL did not become ready within 180 seconds.`n$containerLogs"
    }

    $connectionString = "Host=127.0.0.1;Port=$port;Database=CustomerDb;Username=postgres;Password=$password"
    [Environment]::SetEnvironmentVariable($connectionVariable, $connectionString, 'Process')
    [Environment]::SetEnvironmentVariable($environmentVariable, 'Development', 'Process')

    for ($run = 1; $run -le 2; $run++) {
        & dotnet run `
            --project api/src/Concertable.Customer.Migrations/Concertable.Customer.Migrations.csproj `
            --configuration $Configuration `
            --no-build `
            --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "Customer migration job run $run failed."
        }
    }

    $query = @"
SELECT COUNT(*)
FROM (VALUES
    ('artist', 'Artists'),
    ('concert', 'Concerts'),
    ('preference', 'Preferences'),
    ('review', 'Reviews'),
    ('ticket', 'Tickets'),
    ('user', 'Users'),
    ('venue', 'Venues'),
    ('messaging', 'Inbox'),
    ('messaging', 'Outbox')
) AS expected(schema_name, table_name)
JOIN information_schema.tables AS tables
  ON tables.table_schema = expected.schema_name AND tables.table_name = expected.table_name;
"@

    $tableCount = (& docker exec $containerName psql --username postgres --dbname CustomerDb `
        --tuples-only --no-align --command $query).Trim()
    if ($LASTEXITCODE -ne 0 -or $tableCount -ne '9') {
        throw "Expected all nine Customer migration targets; found '$tableCount'."
    }

    # Every context keeps its own history in the schema it owns. One shared table would mean a context
    # could see another's applied migrations and skip its own.
    $historyQuery = @"
SELECT string_agg(table_schema || '.' || table_name, ',' ORDER BY table_schema, table_name)
FROM information_schema.tables
WHERE table_name LIKE '__EFMigrationsHistory%';
"@

    $histories = (& docker exec $containerName psql --username postgres --dbname CustomerDb `
        --tuples-only --no-align --command $historyQuery).Trim()
    $expectedHistories = @(
        'artist.__EFMigrationsHistory'
        'concert.__EFMigrationsHistory'
        'messaging.__EFMigrationsHistory_Inbox'
        'messaging.__EFMigrationsHistory_Outbox'
        'preference.__EFMigrationsHistory'
        'review.__EFMigrationsHistory'
        'ticket.__EFMigrationsHistory'
        'user.__EFMigrationsHistory'
        'venue.__EFMigrationsHistory'
    ) -join ','
    if ($LASTEXITCODE -ne 0 -or $histories -ne $expectedHistories) {
        throw "Expected one owned history per context.`nExpected: $expectedHistories`nActual:   $histories"
    }

    Write-Host 'Customer migration job created all nine targets with nine owned histories and completed idempotently.'
}
finally {
    [Environment]::SetEnvironmentVariable($connectionVariable, $previousConnection, 'Process')
    [Environment]::SetEnvironmentVariable($environmentVariable, $previousEnvironment, 'Process')
    if ($containerStarted) {
        & docker rm --force $containerName 2>$null | Out-Null
    }
}
