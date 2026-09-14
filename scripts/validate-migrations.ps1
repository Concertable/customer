[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$env:ConnectionStrings__CustomerDb = 'Server=localhost;Database=CustomerCi;User Id=sa;Password=Customer-CI-Only-Password-123!;TrustServerCertificate=True'

$startupProject = 'api/src/Concertable.Customer.Web/Concertable.Customer.Web.csproj'
$contexts = @(
    @{ Name = 'ArtistDbContext'; Project = 'api/src/Modules/Artist/Concertable.Customer.Artist.Infrastructure/Concertable.Customer.Artist.Infrastructure.csproj' }
    @{ Name = 'ConcertDbContext'; Project = 'api/src/Modules/Concert/Concertable.Customer.Concert.Infrastructure/Concertable.Customer.Concert.Infrastructure.csproj' }
    @{ Name = 'PreferenceDbContext'; Project = 'api/src/Modules/Preference/Concertable.Customer.Preference.Infrastructure/Concertable.Customer.Preference.Infrastructure.csproj' }
    @{ Name = 'ReviewDbContext'; Project = 'api/src/Modules/Review/Concertable.Customer.Review.Infrastructure/Concertable.Customer.Review.Infrastructure.csproj' }
    @{ Name = 'TicketDbContext'; Project = 'api/src/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure/Concertable.Customer.Ticket.Infrastructure.csproj' }
    @{ Name = 'UserDbContext'; Project = 'api/src/Modules/User/Concertable.Customer.User.Infrastructure/Concertable.Customer.User.Infrastructure.csproj' }
    @{ Name = 'VenueDbContext'; Project = 'api/src/Modules/Venue/Concertable.Customer.Venue.Infrastructure/Concertable.Customer.Venue.Infrastructure.csproj' }
)

dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    throw 'Failed to restore repository-local .NET tools.'
}

foreach ($context in $contexts) {
    Write-Host "Validating $($context.Name) migration snapshot..."
    dotnet tool run dotnet-ef migrations has-pending-model-changes `
        --project $context.Project `
        --startup-project $startupProject `
        --context $context.Name `
        --configuration Release `
        --no-build

    if ($LASTEXITCODE -ne 0) {
        throw "$($context.Name) has pending model changes or could not be validated."
    }
}
