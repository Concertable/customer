using Concertable.Contracts;

namespace Concertable.Customer.Concert.Domain.Entities;

public sealed class ConcertGenreEntity
{
    public int ConcertId { get; set; }
    public Genre Genre { get; set; }
    public ConcertEntity Concert { get; set; } = null!;
}
