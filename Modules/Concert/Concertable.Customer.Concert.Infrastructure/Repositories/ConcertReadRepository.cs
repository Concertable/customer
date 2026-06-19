using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Repositories;

internal sealed class ConcertReadRepository : ReadRepository<ConcertEntity>, IConcertReadRepository
{
    public ConcertReadRepository(ConcertDbContext context) : base(context) { }

    public override Task<ConcertEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        context.Concerts.Include(c => c.Genres).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<ConcertDto?> GetDtoAsync(int concertId) =>
        context.Concerts
            .AsNoTracking()
            .Where(c => c.Id == concertId)
            .ToDto()
            .FirstOrDefaultAsync();
}
