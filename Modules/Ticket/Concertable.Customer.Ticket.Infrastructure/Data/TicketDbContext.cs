using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Ticket.Infrastructure.Data;

internal class TicketDbContext(
    DbContextOptions<TicketDbContext> options,
    TicketConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<TicketEntity> Tickets => Set<TicketEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
