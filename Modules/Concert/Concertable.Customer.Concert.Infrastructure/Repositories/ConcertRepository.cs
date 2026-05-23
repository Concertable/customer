using Concertable.Customer.Concert.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Repositories;

internal class ConcertRepository(ConcertDbContext context) : IConcertRepository
{
    public Task<ConcertEntity?> GetByIdAsync(int concertId) =>
        context.Concerts.Include(c => c.Genres).FirstOrDefaultAsync(c => c.Id == concertId);

    public async Task AddAsync(ConcertEntity concert) =>
        await context.Concerts.AddAsync(concert);

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
