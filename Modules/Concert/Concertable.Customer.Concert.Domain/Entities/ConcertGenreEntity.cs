using Concertable.Contracts;

namespace Concertable.Customer.Concert.Domain.Entities;

public sealed class ConcertGenreReadModel
{
    public int ConcertId { get; set; }
    public Genre Genre { get; set; }
    public ConcertReadModel Concert { get; set; } = null!;
}
