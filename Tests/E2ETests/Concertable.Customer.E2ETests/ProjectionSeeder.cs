using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Contracts;
using Concertable.Customer.Seeding;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Concertable.Customer.E2ETests;

internal sealed class ProjectionSeeder
{
    private readonly IHost host;
    private readonly IPollingService polling;

    public ProjectionSeeder(IHost host, IPollingService polling)
    {
        this.host = host;
        this.polling = polling;
    }

    public async Task SeedAsync()
    {
        await using var scope = host.Services.CreateAsyncScope();
        var transport = scope.ServiceProvider.GetRequiredService<IBusTransport>();
        var connectionString = scope.ServiceProvider.GetRequiredService<IConfiguration>()
            .GetConnectionString(AppHostConstants.Databases.Customer)
            ?? throw new InvalidOperationException("Customer DB connection string is missing.");
        var now = DateTime.UtcNow;

        await transport.PublishAsync(
            new VenueChangedEvent(1, Guid.Empty, "The Grand Venue", "Test venue", "avatar.jpg", "grandvenue.jpg",
                "Test County", "Test Town", 51.0, 0.0, "thegrandvenue@test.com"),
            Envelope());

        await transport.PublishAsync(
            new ArtistChangedEvent(2, Guid.Empty, "Indie Vibes", "Test artist", "avatar.jpg", "indievibes.jpg",
                "Test County", "Test Town", 51.0, 0.0, "indievibes@test.com",
                [Genre.Rock, Genre.Electronic, Genre.HipHop]),
            Envelope());

        await transport.PublishAsync(
            new ConcertChangedEvent(SeedData.UpcomingConcertId, "Upcoming FlatFee Show", "Test concert", null, null,
                150, 150, 20m, new DateRange(now.AddDays(15), now.AddDays(15).AddHours(3)), now,
                2, "Indie Vibes", 1, "The Grand Venue", 51.0, 0.0,
                [Genre.Rock, Genre.Indie], Guid.Empty),
            Envelope());

        await polling.UntilAsync(async () =>
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM [concert].[Concerts] WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", SeedData.UpcomingConcertId);
            return await cmd.ExecuteScalarAsync() is not null;
        }, timeout: TimeSpan.FromSeconds(60));
    }

    private static MessageEnvelope Envelope() =>
        new(Guid.NewGuid(), "e2e-seed", DateTimeOffset.UtcNow);
}
