namespace Concertable.Customer.Artist.Domain;

public sealed class ArtistGenreEntity
{
    public int ArtistId { get; set; }
    public Genre Genre { get; set; }
    public ArtistEntity Artist { get; set; } = null!;
}
