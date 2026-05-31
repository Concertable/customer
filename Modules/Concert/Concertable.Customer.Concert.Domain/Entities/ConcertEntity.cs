using Concertable.Kernel;

namespace Concertable.Customer.Concert.Domain.Entities;

public class ConcertEntity : IIdEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string? BannerUrl { get; private set; }
    public string? Avatar { get; private set; }
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
    public double AverageRating { get; private set; }
    public int ReviewCount { get; private set; }
    public ICollection<ConcertGenreEntity> Genres { get; private set; } = [];

    private ConcertEntity() { }

    public static ConcertEntity Create(
        int concertId,
        string name,
        string about,
        string? bannerUrl,
        string? avatar,
        int totalTickets,
        decimal price,
        DateRange period,
        DateTime? datePosted,
        int artistId,
        string artistName,
        int venueId,
        string venueName,
        Guid payeeUserId) => new()
    {
        Id = concertId,
        Name = name,
        About = about,
        BannerUrl = bannerUrl,
        Avatar = avatar,
        TotalTickets = totalTickets,
        AvailableTickets = totalTickets,
        Price = price,
        Period = period,
        DatePosted = datePosted,
        ArtistId = artistId,
        ArtistName = artistName,
        VenueId = venueId,
        VenueName = venueName,
        PayeeUserId = payeeUserId
    };

    public void Update(
        string name,
        string about,
        string? bannerUrl,
        string? avatar,
        int totalTickets,
        decimal price,
        DateRange period,
        DateTime? datePosted,
        int artistId,
        string artistName,
        int venueId,
        string venueName,
        Guid payeeUserId)
    {
        var sold = TotalTickets - AvailableTickets;
        Name = name;
        About = about;
        BannerUrl = bannerUrl;
        Avatar = avatar;
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
    }

    public void UpdateRating(double averageRating, int reviewCount)
    {
        AverageRating = averageRating;
        ReviewCount = reviewCount;
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
