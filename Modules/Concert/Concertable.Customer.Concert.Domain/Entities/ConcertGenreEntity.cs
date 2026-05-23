namespace Concertable.Customer.Concert.Domain;

public sealed class ConcertGenreReadModel
{
    public int ConcertId { get; set; }
    public Genre Genre { get; set; }
    public ConcertReadModel Concert { get; set; } = null!;
}
