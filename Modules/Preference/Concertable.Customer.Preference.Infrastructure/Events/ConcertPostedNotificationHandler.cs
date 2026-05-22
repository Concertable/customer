using Concertable.Concert.Contracts.Events;
using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.Customer.Preference.Infrastructure.Notifications;
using Concertable.Messaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Events;

internal class ConcertPostedNotificationHandler : IIntegrationEventHandler<ConcertPostedEvent>
{
    private readonly PreferenceDbContext context;
    private readonly IPreferenceService preferenceService;
    private readonly IConcertPostedNotifier notifier;

    public ConcertPostedNotificationHandler(
        PreferenceDbContext context,
        IPreferenceService preferenceService,
        IConcertPostedNotifier notifier)
    {
        this.context = context;
        this.preferenceService = preferenceService;
        this.notifier = notifier;
    }

    public async Task HandleAsync(ConcertPostedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(ConcertPostedNotificationHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(ConcertPostedNotificationHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        await context.SaveChangesAsync(ct);

        if (e.Latitude is null || e.Longitude is null)
            return;

        var userIds = await preferenceService.GetUserIdsByLocationAndGenresAsync(
            e.Latitude.Value, e.Longitude.Value, e.Genres);

        var payload = new
        {
            e.ConcertId,
            e.Name,
            e.Avatar,
            e.Price,
            StartDate = e.Period.Start,
            EndDate = e.Period.End,
            e.DatePosted
        };

        var tasks = userIds.Select(userId => notifier.ConcertPostedAsync(userId.ToString(), payload));
        await Task.WhenAll(tasks);
    }
}
