using Concertable.Payment.Client;

namespace Concertable.Customer.Ticket.Application.DTOs;

internal record TicketCheckout(
    CheckoutSession Session,
    decimal Price,
    int ConcertId,
    int Quantity);

internal record TicketConcertDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public required string VenueName { get; set; }
    public required string ArtistName { get; set; }
}

internal record TicketDto
{
    public Guid Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public byte[] QrCode { get; set; } = null!;
    public Guid UserId { get; set; }
    public required string UserEmail { get; set; }
    public required TicketConcertDto Concert { get; set; }
}
