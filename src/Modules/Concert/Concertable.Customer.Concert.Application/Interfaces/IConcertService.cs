using Concertable.Customer.Concert.Application.DTOs;
using Concertable.Customer.Concert.Contracts;
using Reunion;

namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertService
{
    Task<Option<ConcertDetails>> GetDetailsByIdAsync(int concertId, CancellationToken ct = default);
    Task<Option<ConcertDto>> GetByIdAsync(int concertId, CancellationToken ct = default);
}
