using Concertable.Customer.Ticket.Domain.Events;
using Concertable.Kernel;

namespace Concertable.Customer.Ticket.Domain.Entities;

public sealed class TicketEntity : IGuidEntity, IEventRaiser
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

    private readonly EventRaiser events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => events.DomainEvents;
    public void ClearDomainEvents() => events.Clear();

    private TicketEntity() { }

    public void MarkReviewed() => HasReview = true;

    public static TicketEntity Purchase(
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
        string venueName)
    {
        var ticket = Create(
            id, userId, concertId, qrCode, purchaseDate,
            concertName, price, period, artistId, artistName, venueId, venueName);
        ticket.events.Raise(new TicketPurchasedDomainEvent(id, userId, concertId, price, purchaseDate));
        return ticket;
    }

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
