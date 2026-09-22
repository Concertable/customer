using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.DataAccess.Infrastructure;
using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Messaging.Infrastructure.Inbox;
using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.Migrations;

public static class CustomerMigrationJob
{
    public static async Task RunAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var contextFactories = new Func<DbContext>[]
        {
            () => new UserDbContextFactory().CreateDbContext(connectionString),
            () => new ArtistDbContextFactory().CreateDbContext(connectionString),
            () => new VenueDbContextFactory().CreateDbContext(connectionString),
            () => new ConcertDbContextFactory().CreateDbContext(connectionString),
            () => new TicketDbContextFactory().CreateDbContext(connectionString),
            () => new ReviewDbContextFactory().CreateDbContext(connectionString),
            () => new PreferenceDbContextFactory().CreateDbContext(connectionString),
            () => new InboxDbContext(new DbContextOptionsBuilder<InboxDbContext>()
                .UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsHistoryTable(
                        MigrationsHistory.InboxTable, MigrationsHistory.MessagingSchema))
                .Options),
            () => new OutboxDbContext(
                new DbContextOptionsBuilder<OutboxDbContext>()
                    .UseNpgsql(
                        connectionString,
                        npgsql => npgsql.MigrationsHistoryTable(
                            MigrationsHistory.OutboxTable, MigrationsHistory.MessagingSchema))
                    .Options,
                Options.Create(new OutboxOptions())),
        };

        foreach (var createContext in contextFactories)
        {
            await using var context = createContext();
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            if ((await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
                throw new InvalidOperationException($"Migrations remain pending for {context.GetType().Name}.");
        }
    }
}
