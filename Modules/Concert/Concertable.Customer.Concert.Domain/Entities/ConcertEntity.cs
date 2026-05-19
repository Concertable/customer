namespace Concertable.Customer.Concert.Domain;

public class ConcertEntity : IIdEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public int TotalTickets { get; private set; }
    public int AvailableTickets { get; private set; }
    public decimal Price { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public DateTime? DatePosted { get; private set; }
    public int ArtistId { get; private set; }
    public string ArtistName { get; private set; } = null!;
    public int VenueId { get; private set; }
    public string VenueName { get; private set; } = null!;
    public Guid PayeeUserId { get; private set; }
    public string ContractType { get; private set; } = null!;

    private ConcertEntity() { }

    public static ConcertEntity Create(
        int concertId,
        string name,
        int totalTickets,
        decimal price,
        DateRange period,
        DateTime? datePosted,
        int artistId,
        string artistName,
        int venueId,
        string venueName,
        Guid payeeUserId,
        string contractType) => new()
    {
        Id = concertId,
        Name = name,
        TotalTickets = totalTickets,
        AvailableTickets = totalTickets,
        Price = price,
        Period = period,
        DatePosted = datePosted,
        ArtistId = artistId,
        ArtistName = artistName,
        VenueId = venueId,
        VenueName = venueName,
        PayeeUserId = payeeUserId,
        ContractType = contractType
    };

    public void Update(
        string name,
        int totalTickets,
        decimal price,
        DateRange period,
        DateTime? datePosted,
        int artistId,
        string artistName,
        int venueId,
        string venueName,
        Guid payeeUserId,
        string contractType)
    {
        var sold = TotalTickets - AvailableTickets;
        Name = name;
        TotalTickets = totalTickets;
        AvailableTickets = Math.Max(0, totalTickets - sold);
        Price = price;
        Period = period;
        DatePosted = datePosted;
        ArtistId = artistId;
        ArtistName = artistName;
        VenueId = venueId;
        VenueName = venueName;
        PayeeUserId = payeeUserId;
        ContractType = contractType;
    }

    public void DecrementAvailability(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (AvailableTickets - quantity < 0)
            throw new DomainException($"Not enough tickets available. Only {AvailableTickets} left.");
        AvailableTickets -= quantity;
    }

    public void RestoreAvailability(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (AvailableTickets + quantity > TotalTickets)
            throw new DomainException($"Restore would exceed capacity ({TotalTickets}).");
        AvailableTickets += quantity;
    }
}
