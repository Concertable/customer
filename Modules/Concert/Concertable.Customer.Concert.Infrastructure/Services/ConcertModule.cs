using Concertable.Customer.Concert.Application.Interfaces;
using Concertable.Customer.Concert.Contracts;

namespace Concertable.Customer.Concert.Infrastructure.Services;

internal sealed class ConcertModule : IConcertModule
{
    private readonly IConcertReadRepository concertRepository;

    public ConcertModule(IConcertReadRepository concertRepository)
    {
        this.concertRepository = concertRepository;
    }

    public Task<ConcertDto?> GetByIdAsync(int concertId, CancellationToken ct = default) =>
        concertRepository.GetDtoAsync(concertId);
}
