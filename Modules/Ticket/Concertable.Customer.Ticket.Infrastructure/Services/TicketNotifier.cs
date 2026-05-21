namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal class TicketNotifier : ITicketNotifier
{
    private readonly INotificationModule notifications;

    public TicketNotifier(INotificationModule notifications)
    {
        this.notifications = notifications;
    }

    public Task TicketPurchasedAsync(string userId, object payload) =>
        notifications.SendAsync(userId, "TicketPurchased", payload);

    public Task TicketPurchaseFailedAsync(string userId, object payload) =>
        notifications.SendAsync(userId, "TicketPurchaseFailed", payload);
}
