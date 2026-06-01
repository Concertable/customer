namespace Concertable.Customer.Ticket.Application.Requests;

internal sealed class TicketPurchaseParams
{
    public required string PaymentMethodId { get; set; }
    public int ConcertId { get; set; }
    public int Quantity { get; set; } = 1;
}
