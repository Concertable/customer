namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketNotifier
{
    Task TicketPurchasedAsync(string userId, object payload);
}
