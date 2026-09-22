using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.User.Infrastructure.Data;

internal sealed class UserDbContext : DbContextBase
{
    private readonly UserConfigurationProvider provider;

    public UserDbContext(
        DbContextOptions<UserDbContext> options,
        IOptions<OutboxOptions> outboxOptions,
        UserConfigurationProvider provider)
        : base(options, outboxOptions)
    {
        this.provider = provider;
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
