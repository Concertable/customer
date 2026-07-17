using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal sealed class ArtistDbContextFactory : CustomerDesignTimeDbContextFactory<ArtistDbContext>
{
    protected override ArtistDbContext Create(DbContextOptions<ArtistDbContext> options) =>
        new(options, new ArtistConfigurationProvider());
}
