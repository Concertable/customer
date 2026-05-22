namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal class TicketNotifier : ITicketNotifier
{
    private readonly INotificationClient notificationClient;

    public TicketNotifier(INotificationClient notificationClient)
    {
        this.notificationClient = notificationClient;
    }

    public Task TicketPurchasedAsync(string userId, object payload) =>
        notificationClient.SendAsync(userId, "TicketPurchased", payload);

    public Task TicketPurchaseFailedAsync(string userId, object payload) =>
        notificationClient.SendAsync(userId, "TicketPurchaseFailed", payload);
}
