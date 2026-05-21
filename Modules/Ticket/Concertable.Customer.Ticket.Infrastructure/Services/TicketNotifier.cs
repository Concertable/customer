namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal class TicketNotifier : ITicketNotifier
{
    private readonly INotificationModule notification;

    public TicketNotifier(INotificationModule notification)
    {
        this.notification = notification;
    }

    public Task TicketPurchasedAsync(string userId, object payload) =>
        notification.SendAsync(userId, "TicketPurchased", payload);

    public Task TicketPurchaseFailedAsync(string userId, object payload) =>
        notification.SendAsync(userId, "TicketPurchaseFailed", payload);
}
