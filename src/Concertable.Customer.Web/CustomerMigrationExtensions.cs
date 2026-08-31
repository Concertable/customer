using Concertable.Customer.Artist.Api.Extensions;
using Concertable.Customer.Concert.Api.Extensions;
using Concertable.Customer.Preference.Api.Extensions;
using Concertable.Customer.Review.Api.Extensions;
using Concertable.Customer.Ticket.Api.Extensions;
using Concertable.Customer.User.Api.Extensions;
using Concertable.Customer.Venue.Api.Extensions;
using Concertable.Messaging.Infrastructure.Inbox;
using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Web;

public static class CustomerMigrationExtensions
{
    extension(IServiceProvider services)
    {
        public async Task MigrateCustomerDatabaseAsync(CancellationToken cancellationToken = default)
        {
            await services.GetRequiredService<OutboxDbContext>().Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            await services.GetRequiredService<InboxDbContext>().Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            await services.MigrateArtistModuleAsync(cancellationToken).ConfigureAwait(false);
            await services.MigrateConcertModuleAsync(cancellationToken).ConfigureAwait(false);
            await services.MigratePreferenceModuleAsync(cancellationToken).ConfigureAwait(false);
            await services.MigrateReviewModuleAsync(cancellationToken).ConfigureAwait(false);
            await services.MigrateTicketModuleAsync(cancellationToken).ConfigureAwait(false);
            await services.MigrateUserModuleAsync(cancellationToken).ConfigureAwait(false);
            await services.MigrateVenueModuleAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
