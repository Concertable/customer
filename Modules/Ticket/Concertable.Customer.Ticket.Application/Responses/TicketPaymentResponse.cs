using Concertable.Payment.Client;

namespace Concertable.Customer.Ticket.Application.Responses;

internal sealed record TicketPaymentResponse : PaymentResponse
{
    public IReadOnlyList<Guid> TicketIds { get; init; } = [];
    public int ConcertId { get; init; }
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public DateTime PurchaseDate { get; init; }
    public string? UserEmail { get; init; }
}
