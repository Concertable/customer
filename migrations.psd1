@{
    Environment = @{
        ConnectionStrings__CustomerDb = 'Server=localhost;Database=concertable-customer;Trusted_Connection=True;TrustServerCertificate=True'
    }
    Migrations = @(
        @{ Context = 'ConcertDbContext'; Project = 'src/Modules/Concert/Concertable.Customer.Concert.Infrastructure'; StartupProject = 'src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'TicketDbContext'; Project = 'src/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure'; StartupProject = 'src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'ReviewDbContext'; Project = 'src/Modules/Review/Concertable.Customer.Review.Infrastructure'; StartupProject = 'src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'UserDbContext'; Project = 'src/Modules/User/Concertable.Customer.User.Infrastructure'; StartupProject = 'src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'PreferenceDbContext'; Project = 'src/Modules/Preference/Concertable.Customer.Preference.Infrastructure'; StartupProject = 'src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'VenueDbContext'; Project = 'src/Modules/Venue/Concertable.Customer.Venue.Infrastructure'; StartupProject = 'src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
        @{ Context = 'ArtistDbContext'; Project = 'src/Modules/Artist/Concertable.Customer.Artist.Infrastructure'; StartupProject = 'src/Concertable.Customer.Web'; OutputDir = 'Data/Migrations' }
    )
}
