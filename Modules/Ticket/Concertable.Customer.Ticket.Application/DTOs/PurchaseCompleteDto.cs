namespace Concertable.Customer.Ticket.Application.DTOs;

internal sealed record PurchaseCompleteDto
{
    public int EntityId { get; set; }
    public Guid FromUserId { get; set; }
    public required string FromEmail { get; set; }
    public int? Quantity { get; set; }
}
