namespace Concertable.Customer.Preference.Infrastructure.Notifications;

internal interface IConcertPostedNotifier
{
    Task ConcertPostedAsync(string userId, object payload);
}
