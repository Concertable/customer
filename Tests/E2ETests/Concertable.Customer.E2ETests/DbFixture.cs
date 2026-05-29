using Aspire.Hosting;
using Respawn;
using Respawn.Graph;
using ConcertSchema = Concertable.Customer.Concert.Infrastructure.Schema;
using ArtistSchema = Concertable.Customer.Artist.Infrastructure.Schema;
using VenueSchema = Concertable.Customer.Venue.Infrastructure.Schema;
using UserSchema = Concertable.Customer.User.Infrastructure.Schema;
using MessagingSchema = Concertable.Messaging.Infrastructure.Schema;

namespace Concertable.Customer.E2ETests;

public sealed class DbFixture
{
    private readonly DistributedApplication app;
    private readonly RespawnableDb customer = new();
    private readonly PaymentDbFixture payment = new();

    public PaymentDb Payment => payment.Payment;

    public DbFixture(DistributedApplication app) => this.app = app;

    public async Task InitializeAsync()
    {
        await customer.InitializeAsync(app, AppHostConstants.Databases.Customer, new RespawnerOptions
        {
            TablesToIgnore = [
                "__EFMigrationsHistory",
                new Table(ConcertSchema.Name, ConcertSchema.Concerts),
                new Table(ConcertSchema.Name, ConcertSchema.ConcertGenres),
                new Table(ArtistSchema.Name, ArtistSchema.Artists),
                new Table(ArtistSchema.Name, ArtistSchema.ArtistGenres),
                new Table(VenueSchema.Name, VenueSchema.Venues),
                new Table(UserSchema.Name, UserSchema.Users),
                new Table(MessagingSchema.Name, MessagingSchema.Inbox),
            ],
            DbAdapter = DbAdapter.SqlServer,
            WithReseed = true
        });
        await payment.InitializeAsync(app);
    }

    public async Task ResetAsync()
    {
        await customer.ResetAsync();
        await payment.ResetAsync();
    }

    public async Task DisposeAsync()
    {
        await customer.DisposeAsync();
        await payment.DisposeAsync();
    }
}
