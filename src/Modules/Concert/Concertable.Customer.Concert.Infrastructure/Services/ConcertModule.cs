using Concertable.Customer.Concert.Application.Interfaces;
using Concertable.Customer.Concert.Contracts;
using Reunion;

namespace Concertable.Customer.Concert.Infrastructure.Services;

internal sealed class ConcertModule : IConcertModule
{
    private readonly IConcertService concertService;

    public ConcertModule(IConcertService concertService)
    {
        this.concertService = concertService;
    }

    public Task<Option<ConcertDto>> GetByIdAsync(int concertId, CancellationToken ct = default) =>
        concertService.GetByIdAsync(concertId, ct);
}
