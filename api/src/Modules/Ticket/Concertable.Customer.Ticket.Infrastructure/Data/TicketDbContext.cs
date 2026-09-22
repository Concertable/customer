using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.Ticket.Infrastructure.Data;

internal sealed class TicketDbContext : DbContextBase
{
    private readonly TicketConfigurationProvider provider;

    public TicketDbContext(
        DbContextOptions<TicketDbContext> options,
        IOptions<OutboxOptions> outboxOptions,
        TicketConfigurationProvider provider)
        : base(options, outboxOptions)
    {
        this.provider = provider;
    }

    public DbSet<TicketEntity> Tickets => Set<TicketEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
