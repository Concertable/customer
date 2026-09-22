using Concertable.Customer.Review.Domain.Entities;
using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal sealed class ReviewDbContext : DbContextBase
{
    private readonly ReviewConfigurationProvider provider;

    public ReviewDbContext(
        DbContextOptions<ReviewDbContext> options,
        IOptions<OutboxOptions> outboxOptions,
        ReviewConfigurationProvider provider)
        : base(options, outboxOptions)
    {
        this.provider = provider;
    }

    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
