using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal class ArtistReadRepository : IArtistReadRepository
{
    private readonly ArtistDbContext context;

    public ArtistReadRepository(ArtistDbContext context)
    {
        this.context = context;
    }

    public Task<ArtistReadModel?> GetByIdAsync(int artistId) =>
        context.Artists.Include(a => a.Genres).FirstOrDefaultAsync(a => a.Id == artistId);
}
