using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Kernel;
using Microsoft.EntityFrameworkCore;
using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Venue.Infrastructure.Handlers;

internal sealed class VenueProjectionHandler : IIntegrationEventHandler<VenueChangedEvent>
{
    private readonly VenueDbContext context;

    public VenueProjectionHandler(VenueDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(VenueChangedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(VenueProjectionHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(VenueProjectionHandler));

        var venue = await context.Venues.FirstOrDefaultAsync(v => v.Id == e.VenueId, ct);

        if (venue is null)
        {
            venue = VenueEntity.Create(
                e.VenueId,
                e.UserId,
                e.Name,
                e.About,
                e.Avatar,
                e.BannerUrl,
                new Address(e.County, e.Town),
                e.Latitude,
                e.Longitude,
                e.Email);
            context.Venues.Add(venue);
        }
        else
        {
            venue.Update(
                e.UserId,
                e.Name,
                e.About,
                e.Avatar,
                e.BannerUrl,
                new Address(e.County, e.Town),
                e.Latitude,
                e.Longitude,
                e.Email);
        }

        await context.SaveChangesAsync(ct);
    }
}
