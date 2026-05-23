using Concertable.Customer.Concert.Application.Dtos;

namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertService
{
    Task<ConcertDetailDto?> GetByIdAsync(int concertId, CancellationToken ct = default);
}
