using Concertable.Payment.Contracts;

namespace Concertable.Customer.Ticket.Application.DTOs;

internal sealed record PurchaseComplete
{
    public required PaymentOperationReference Reference { get; init; }
    public int EntityId { get; init; }
    public Guid FromUserId { get; init; }
    public string? FromEmail { get; init; }
    public int? Quantity { get; init; }
}
