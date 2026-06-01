namespace Concertable.Customer.IntegrationTests.Fixtures.Mocks;

public sealed class MockNotificationClient : IMockNotificationClient
{
    public List<(string UserId, object Payload)> ConcertPosted { get; } = [];
    public List<(string UserId, object Payload)> TicketPurchased { get; } = [];
    public List<(string UserId, string EventName, object Payload)> Other { get; } = [];

    public Task SendAsync(string userId, string eventName, object payload)
    {
        switch (eventName)
        {
            case "ConcertPosted":
                ConcertPosted.Add((userId, payload));
                break;
            case "TicketPurchased":
                TicketPurchased.Add((userId, payload));
                break;
            default:
                Other.Add((userId, eventName, payload));
                break;
        }
        return Task.CompletedTask;
    }

    public void Reset()
    {
        ConcertPosted.Clear();
        TicketPurchased.Clear();
        Other.Clear();
    }
}
