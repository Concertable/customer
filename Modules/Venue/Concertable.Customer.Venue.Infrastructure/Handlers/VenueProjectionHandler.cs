using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Messaging.Domain;
using Concertable.B2B.Venue.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Venue.Infrastructure.Handlers;

internal class VenueProjectionHandler : IIntegrationEventHandler<VenueChangedEvent>
{
    private readonly VenueDbContext context;

    public VenueProjectionHandler(VenueDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(VenueChangedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(VenueProjectionHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(VenueProjectionHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        var venue = await context.Venues.FirstOrDefaultAsync(v => v.Id == e.VenueId, ct);

        if (venue is null)
        {
            venue = VenueReadModel.Create(
                e.VenueId,
                e.UserId,
                e.Name,
                e.About,
                e.Avatar,
                e.BannerUrl,
                e.County,
                e.Town,
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
                e.County,
                e.Town,
                e.Latitude,
                e.Longitude,
                e.Email);
        }

        await context.SaveChangesAsync(ct);
    }
}
