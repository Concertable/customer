using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal sealed class VenueReadModelProjectionHandler : IIntegrationEventHandler<VenueChangedEvent>
{
    private readonly ConcertDbContext context;

    public VenueReadModelProjectionHandler(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(VenueChangedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(VenueReadModelProjectionHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(VenueReadModelProjectionHandler));

        var venue = await context.VenueReadModels.FirstOrDefaultAsync(v => v.Id == e.VenueId, ct);

        if (venue is null)
        {
            context.VenueReadModels.Add(new VenueReadModel
            {
                Id = e.VenueId,
                Name = e.Name,
                Address = new Address(e.County, e.Town),
                Latitude = e.Latitude,
                Longitude = e.Longitude
            });
        }
        else
        {
            venue.Name = e.Name;
            venue.Address = new Address(e.County, e.Town);
            venue.Latitude = e.Latitude;
            venue.Longitude = e.Longitude;
        }

        await context.SaveChangesAsync(ct);
    }
}
