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

try {
    & docker run --detach --rm --name $containerName `
        --env 'ACCEPT_EULA=Y' `
        --env "MSSQL_SA_PASSWORD=$password" `
        --publish '127.0.0.1::1433' `
        'mcr.microsoft.com/mssql/server:2022-latest' | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'Failed to start the temporary SQL Server container.'
    }
    $containerStarted = $true

    $portOutput = & docker port $containerName '1433/tcp'
    if ($LASTEXITCODE -ne 0 -or $portOutput -notmatch ':(?<port>\d+)\s*$') {
        throw "Could not resolve the temporary SQL Server port: $portOutput"
    }
    $port = $Matches.port

    $ready = $false
    for ($attempt = 1; $attempt -le 180; $attempt++) {
        & docker exec $containerName /opt/mssql-tools18/bin/sqlcmd `
            -S localhost -U sa -P $password -C -Q 'SELECT 1' 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            break
        }

        Start-Sleep -Seconds 1
    }

    if (-not $ready) {
        $containerLogs = & docker logs $containerName 2>&1
        throw "Temporary SQL Server did not become ready within 180 seconds.`n$containerLogs"
    }

    $connectionString = "Server=127.0.0.1,$port;Database=CustomerDb;User Id=sa;Password=$password;Encrypt=False;TrustServerCertificate=True"
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
SET NOCOUNT ON;
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
JOIN sys.schemas AS schemas ON schemas.name = expected.schema_name
JOIN sys.tables AS tables ON tables.schema_id = schemas.schema_id AND tables.name = expected.table_name;
"@

    $tableCount = (& docker exec $containerName /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U sa -P $password -C -d CustomerDb -h -1 -W -Q $query).Trim()
    if ($LASTEXITCODE -ne 0 -or $tableCount -ne '9') {
        throw "Expected all nine Customer migration targets; found '$tableCount'."
    }

    Write-Host 'Customer migration job created all nine targets and completed idempotently.'
}
finally {
    [Environment]::SetEnvironmentVariable($connectionVariable, $previousConnection, 'Process')
    [Environment]::SetEnvironmentVariable($environmentVariable, $previousEnvironment, 'Process')
    if ($containerStarted) {
        & docker rm --force $containerName 2>$null | Out-Null
    }
}
