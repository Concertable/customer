using Concertable.Payment.Client;

namespace Concertable.Customer.Ticket.Application.Responses;

internal record TicketPaymentResponse : PaymentResponse
{
    public IReadOnlyList<Guid> TicketIds { get; set; } = [];
    public int ConcertId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? UserEmail { get; set; }
}
