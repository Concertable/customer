using Concertable.Customer.Concert.Domain.Entities;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertReadRepository : IReadRepository<ConcertEntity>
{
    Task SaveChangesAsync();
}
