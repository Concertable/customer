using Concertable.Customer.Concert.Application.DTOs;

namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertService
{
    Task<ConcertDetails?> GetDetailsByIdAsync(int concertId, CancellationToken ct = default);
}
