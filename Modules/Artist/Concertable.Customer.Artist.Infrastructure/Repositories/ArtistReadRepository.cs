using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal class ArtistReadRepository : ReadRepository<ArtistEntity>, IArtistReadRepository
{
    public ArtistReadRepository(ArtistDbContext context) : base(context) { }

    public override Task<ArtistEntity?> GetByIdAsync(int id) =>
        context.Artists.Include(a => a.Genres).FirstOrDefaultAsync(a => a.Id == id);
}
