using Concertable.Customer.Concert.Application.DTOs;
using Reunion;

namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertService
{
    Task<Option<ConcertDetails>> GetDetailsByIdAsync(int concertId, CancellationToken ct = default);
}
