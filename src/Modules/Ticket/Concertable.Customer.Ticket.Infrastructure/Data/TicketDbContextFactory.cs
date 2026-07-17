using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Ticket.Infrastructure.Data;

internal sealed class TicketDbContextFactory : CustomerDesignTimeDbContextFactory<TicketDbContext>
{
    protected override TicketDbContext Create(DbContextOptions<TicketDbContext> options) =>
        new(options, new TicketConfigurationProvider());
}
