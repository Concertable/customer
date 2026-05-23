namespace Concertable.Customer.Artist.Domain;

public class ArtistEntity : IIdEntity
{
    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string About { get; private set; } = null!;
    public string Avatar { get; private set; } = null!;
    public string BannerUrl { get; private set; } = null!;
    public string County { get; private set; } = null!;
    public string Town { get; private set; } = null!;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Email { get; private set; } = null!;
    public double AverageRating { get; private set; }
    public int ReviewCount { get; private set; }
    public ICollection<ArtistGenreEntity> Genres { get; private set; } = [];

    private ArtistEntity() { }

    public static ArtistEntity Create(
        int artistId,
        Guid userId,
        string name,
        string about,
        string avatar,
        string bannerUrl,
        string county,
        string town,
        double latitude,
        double longitude,
        string email) => new()
    {
        Id = artistId,
        UserId = userId,
        Name = name,
        About = about,
        Avatar = avatar,
        BannerUrl = bannerUrl,
        County = county,
        Town = town,
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
        string county,
        string town,
        double latitude,
        double longitude,
        string email)
    {
        UserId = userId;
        Name = name;
        About = about;
        Avatar = avatar;
        BannerUrl = bannerUrl;
        County = county;
        Town = town;
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
