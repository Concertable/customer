using Concertable.Customer.Concert.Application.DTOs;
using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertReadRepository : IReadRepository<ConcertEntity>
{
    Task<ConcertDto?> GetDtoAsync(int concertId);

    Task<ConcertDetails?> GetDetailsAsync(int concertId, CancellationToken ct = default);
}
