namespace Concertable.Customer.TestKit;

public sealed record CustomerTicket
{
    public Guid Id { get; init; }
    public DateTime PurchaseDate { get; init; }
    public byte[] QrCode { get; init; } = null!;
    public Guid UserId { get; init; }
    public required CustomerTicketConcert Concert { get; init; }
}
