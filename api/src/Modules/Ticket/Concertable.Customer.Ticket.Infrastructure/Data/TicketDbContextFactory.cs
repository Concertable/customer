using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Ticket.Infrastructure.Data;

internal sealed class TicketDbContextFactory : CustomerDesignTimeDbContextFactory<TicketDbContext>
{
    protected override string MigrationsSchema => Schema.Name;

    protected override TicketDbContext Create(DbContextOptions<TicketDbContext> options) =>
        new(options, DefaultOutboxOptions, new TicketConfigurationProvider());
}
