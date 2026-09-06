namespace Concertable.Customer.Ticket.Application.Requests;

internal sealed class TicketPurchaseParams
{
    public int ConcertId { get; init; }
    public int Quantity { get; init; } = 1;
}
