namespace Concertable.Customer.Preference.Infrastructure.Notifications;

internal class ConcertPostedNotifier : IConcertPostedNotifier
{
    private readonly INotificationClient notificationClient;

    public ConcertPostedNotifier(INotificationClient notificationClient)
    {
        this.notificationClient = notificationClient;
    }

    public Task ConcertPostedAsync(string userId, object payload) =>
        notificationClient.SendAsync(userId, "ConcertPosted", payload);
}
