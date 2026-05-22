using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.User.Infrastructure.Data;

internal class UserDbContext(
    DbContextOptions<UserDbContext> options,
    UserConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
