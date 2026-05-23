namespace Concertable.Customer.Artist.Domain;

public sealed class ArtistGenreReadModel
{
    public int ArtistId { get; set; }
    public Genre Genre { get; set; }
    public ArtistReadModel Artist { get; set; } = null!;
}
