using Concertable.Payment.Client;

namespace Concertable.Customer.Ticket.Application.DTOs;

internal sealed record TicketCheckout(
    CheckoutSession Session,
    decimal Price,
    int ConcertId,
    int Quantity);

internal sealed record TicketConcert
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public decimal Price { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public required string VenueName { get; init; }
    public required string ArtistName { get; init; }
}

internal sealed record TicketDto
{
    public Guid Id { get; init; }
    public DateTime PurchaseDate { get; init; }
    public byte[] QrCode { get; init; } = null!;
    public Guid UserId { get; init; }
    public required string UserEmail { get; init; }
    public required TicketConcert Concert { get; init; }
}
