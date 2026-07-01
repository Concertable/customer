namespace Concertable.Customer.Ticket.Application.DTOs;

internal sealed record PurchaseComplete
{
    public int EntityId { get; init; }
    public Guid FromUserId { get; init; }
    public required string FromEmail { get; init; }
    public int? Quantity { get; init; }
}
