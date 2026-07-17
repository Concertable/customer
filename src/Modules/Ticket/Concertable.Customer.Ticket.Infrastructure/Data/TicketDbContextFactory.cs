using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Ticket.Infrastructure.Data;

internal sealed class TicketDbContextFactory : IDesignTimeDbContextFactory<TicketDbContext>
{
    public TicketDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConnectionString.Customer();
        var options = new DbContextOptionsBuilder<TicketDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new TicketDbContext(options, new TicketConfigurationProvider());
    }
}
