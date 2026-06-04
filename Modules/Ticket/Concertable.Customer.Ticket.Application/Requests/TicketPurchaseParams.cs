namespace Concertable.Customer.Ticket.Application.Requests;

internal sealed class TicketPurchaseParams
{
    public required string PaymentMethodId { get; init; }
    public int ConcertId { get; init; }
    public int Quantity { get; init; } = 1;
}
