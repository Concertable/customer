using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Repositories;

internal class ConcertReadRepository : IConcertReadRepository
{
    private readonly ConcertDbContext context;

    public ConcertReadRepository(ConcertDbContext context)
    {
        this.context = context;
    }

    public Task<ConcertReadModel?> GetByIdAsync(int concertId) =>
        context.Concerts.Include(c => c.Genres).FirstOrDefaultAsync(c => c.Id == concertId);

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
