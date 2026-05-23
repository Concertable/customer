using Concertable.Customer.Artist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal class ArtistRepository : IArtistRepository
{
    private readonly ArtistDbContext context;

    public ArtistRepository(ArtistDbContext context)
    {
        this.context = context;
    }

    public Task<ArtistEntity?> GetByIdAsync(int artistId) =>
        context.Artists.Include(a => a.Genres).FirstOrDefaultAsync(a => a.Id == artistId);

    public async Task AddAsync(ArtistEntity artist) =>
        await context.Artists.AddAsync(artist);

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
