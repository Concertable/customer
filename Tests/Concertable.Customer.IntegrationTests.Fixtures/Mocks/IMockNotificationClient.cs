using Concertable.Kernel.Notifications;
using Concertable.Testing.Integration;

namespace Concertable.Customer.IntegrationTests.Fixtures.Mocks;

public interface IMockNotificationClient : INotificationClient, IResettable
{
    List<(string UserId, object Payload)> ConcertPosted { get; }
    List<(string UserId, object Payload)> TicketPurchased { get; }
    List<(string UserId, string EventName, object Payload)> Other { get; }
}
