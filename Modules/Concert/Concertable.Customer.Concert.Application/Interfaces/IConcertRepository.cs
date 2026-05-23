using Concertable.Customer.Concert.Domain.Entities;

namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertReadRepository
{
    Task<ConcertReadModel?> GetByIdAsync(int concertId);
    Task SaveChangesAsync();
}
