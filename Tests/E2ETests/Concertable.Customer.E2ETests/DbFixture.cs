using Aspire.Hosting;
using Respawn;
using Respawn.Graph;

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
                new Table("Concerts", "concert"),
                new Table("ConcertGenres", "concert"),
                new Table("Artists", "artist"),
                new Table("ArtistGenres", "artist"),
                new Table("Venues", "venue"),
                new Table("Users", "user"),
                new Table("Inbox", "messaging"),
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
