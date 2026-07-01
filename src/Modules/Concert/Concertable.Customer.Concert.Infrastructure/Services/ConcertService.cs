using Concertable.Customer.Concert.Application.DTOs;

namespace Concertable.Customer.Concert.Infrastructure.Services;

internal sealed class ConcertService : IConcertService
{
    private readonly IConcertReadRepository concertRepository;

    public ConcertService(IConcertReadRepository concertRepository)
    {
        this.concertRepository = concertRepository;
    }

    public Task<ConcertDetails?> GetDetailsByIdAsync(int concertId, CancellationToken ct = default) =>
        concertRepository.GetDetailsAsync(concertId, ct);
}
