using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.Customer.Preference.Infrastructure.Notifications;

namespace Concertable.Customer.Preference.Infrastructure.Events;

internal sealed class ConcertPostedNotificationHandler : IIntegrationEventHandler<ConcertPostedEvent>
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
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertPostedNotificationHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(ConcertPostedNotificationHandler));

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
