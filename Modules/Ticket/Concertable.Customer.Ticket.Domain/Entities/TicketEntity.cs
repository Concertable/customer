using Concertable.Kernel;

namespace Concertable.Customer.Ticket.Domain.Entities;

public sealed class TicketEntity : IGuidEntity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int ConcertId { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public byte[] QrCode { get; private set; } = null!;

    public string ConcertName { get; private set; } = null!;
    public decimal Price { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public int ArtistId { get; private set; }
    public string ArtistName { get; private set; } = null!;
    public int VenueId { get; private set; }
    public string VenueName { get; private set; } = null!;
    public bool HasReview { get; private set; }

    private TicketEntity() { }

    public void MarkReviewed() => HasReview = true;

    public static TicketEntity Create(
        Guid id,
        Guid userId,
        int concertId,
        byte[] qrCode,
        DateTime purchaseDate,
        string concertName,
        decimal price,
        DateRange period,
        int artistId,
        string artistName,
        int venueId,
        string venueName) => new()
        {
            Id = id,
            UserId = userId,
            ConcertId = concertId,
            QrCode = qrCode,
            PurchaseDate = purchaseDate,
            ConcertName = concertName,
            Price = price,
            Period = period,
            ArtistId = artistId,
            ArtistName = artistName,
            VenueId = venueId,
            VenueName = venueName
        };
}
