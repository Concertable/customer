using Concertable.Payment.Contracts;

namespace Concertable.Customer.Ticket.Application.DTOs;

internal sealed record TicketPayment : PaymentOutcome
{
    public IReadOnlyList<Guid> TicketIds { get; init; } = [];
    public int ConcertId { get; init; }
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public DateTime PurchaseDate { get; init; }
    public string? UserEmail { get; init; }
}
