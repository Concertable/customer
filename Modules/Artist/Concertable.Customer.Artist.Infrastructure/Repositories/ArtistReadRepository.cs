using Concertable.Customer.Artist.Contracts;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Artist.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal sealed class ArtistReadRepository : ReadRepository<ArtistEntity>, IArtistReadRepository
{
    public ArtistReadRepository(ArtistDbContext context) : base(context) { }

    public override Task<ArtistEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        context.Artists.Include(a => a.Genres).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<ArtistSummary?> GetSummaryAsync(int artistId) =>
        context.Artists
            .Where(a => a.Id == artistId)
            .ToSummary()
            .FirstOrDefaultAsync();
}
