@{
    Environment = @{
        ConnectionStrings__CustomerDb = 'Server=localhost;Database=concertable-customer;Trusted_Connection=True;TrustServerCertificate=True'
    }
    Migrations = @(
        @{ Context = 'ConcertDbContext'; Project = 'api/src/Modules/Concert/Concertable.Customer.Concert.Infrastructure'; StartupProject = 'api/src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'TicketDbContext'; Project = 'api/src/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure'; StartupProject = 'api/src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'ReviewDbContext'; Project = 'api/src/Modules/Review/Concertable.Customer.Review.Infrastructure'; StartupProject = 'api/src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'UserDbContext'; Project = 'api/src/Modules/User/Concertable.Customer.User.Infrastructure'; StartupProject = 'api/src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'PreferenceDbContext'; Project = 'api/src/Modules/Preference/Concertable.Customer.Preference.Infrastructure'; StartupProject = 'api/src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'VenueDbContext'; Project = 'api/src/Modules/Venue/Concertable.Customer.Venue.Infrastructure'; StartupProject = 'api/src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'ArtistDbContext'; Project = 'api/src/Modules/Artist/Concertable.Customer.Artist.Infrastructure'; StartupProject = 'api/src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
    )
}
