using Concertable.Customer.Artist.Contracts;
using Concertable.Customer.Artist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Services;

internal class CustomerArtistModule : ICustomerArtistModule
{
    private readonly ArtistDbContext context;

    public CustomerArtistModule(ArtistDbContext context)
    {
        this.context = context;
    }

    public async Task<ArtistSummary?> GetSummaryAsync(int artistId, CancellationToken ct = default)
    {
        var artist = await context.Artists
            .Include(a => a.Genres)
            .FirstOrDefaultAsync(a => a.Id == artistId, ct);

        if (artist is null)
            return null;

        return new ArtistSummary(
            artist.Id,
            artist.Name,
            artist.Avatar,
            artist.AverageRating,
            artist.County,
            artist.Town,
            artist.Genres.Select(g => g.Genre).ToArray());
    }
}
