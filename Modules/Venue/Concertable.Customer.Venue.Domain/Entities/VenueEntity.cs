namespace Concertable.Customer.Venue.Domain.Entities;

public sealed class VenueEntity : IIdEntity
{
    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string Avatar { get; private set; } = null!;
    public string BannerUrl { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Email { get; private set; } = null!;
    public double AverageRating { get; private set; }
    public int ReviewCount { get; private set; }

    private VenueEntity() { }

    public static VenueEntity Create(
        int venueId,
        Guid userId,
        string name,
        string about,
        string avatar,
        string bannerUrl,
        Address address,
        double latitude,
        double longitude,
        string email) => new()
        {
            Id = venueId,
            UserId = userId,
            Name = name,
            About = about,
            Avatar = avatar,
            BannerUrl = bannerUrl,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            Email = email
        };

    public void Update(
        Guid userId,
        string name,
        string about,
        string avatar,
        string bannerUrl,
        Address address,
        double latitude,
        double longitude,
        string email)
    {
        UserId = userId;
        Name = name;
        About = about;
        Avatar = avatar;
        BannerUrl = bannerUrl;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        Email = email;
    }

    public void UpdateRating(double averageRating, int reviewCount)
    {
        AverageRating = averageRating;
        ReviewCount = reviewCount;
    }
}
