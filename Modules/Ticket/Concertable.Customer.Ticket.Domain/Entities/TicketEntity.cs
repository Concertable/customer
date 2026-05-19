namespace Concertable.Customer.Ticket.Domain;

public class TicketEntity : IGuidEntity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int ConcertId { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public byte[] QrCode { get; private set; } = null!;

    public string ConcertName { get; private set; } = null!;
    public decimal Price { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public string VenueName { get; private set; } = null!;
    public string ArtistName { get; private set; } = null!;

    private TicketEntity() { }

    public static TicketEntity Create(
        Guid id,
        Guid userId,
        int concertId,
        byte[] qrCode,
        DateTime purchaseDate,
        string concertName,
        decimal price,
        DateRange period,
        string venueName,
        string artistName) => new()
    {
        Id = id,
        UserId = userId,
        ConcertId = concertId,
        QrCode = qrCode,
        PurchaseDate = purchaseDate,
        ConcertName = concertName,
        Price = price,
        Period = period,
        VenueName = venueName,
        ArtistName = artistName
    };
}
