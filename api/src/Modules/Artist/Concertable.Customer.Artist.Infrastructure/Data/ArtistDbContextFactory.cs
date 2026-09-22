using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal sealed class ArtistDbContextFactory : CustomerDesignTimeDbContextFactory<ArtistDbContext>
{
    protected override string MigrationsSchema => Schema.Name;

    protected override ArtistDbContext Create(DbContextOptions<ArtistDbContext> options) =>
        new(options, DefaultOutboxOptions, new ArtistConfigurationProvider());
}
