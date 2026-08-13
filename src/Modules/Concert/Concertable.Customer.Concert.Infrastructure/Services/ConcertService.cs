using Concertable.Customer.Concert.Application.DTOs;
using Reunion;

namespace Concertable.Customer.Concert.Infrastructure.Services;

internal sealed class ConcertService : IConcertService
{
    private readonly IConcertReadRepository concertRepository;

    public ConcertService(IConcertReadRepository concertRepository)
    {
        this.concertRepository = concertRepository;
    }

    public Task<Option<ConcertDetails>> GetDetailsByIdAsync(int concertId, CancellationToken ct = default) =>
        concertRepository.GetDetailsAsync(concertId, ct).ToOption();
}
