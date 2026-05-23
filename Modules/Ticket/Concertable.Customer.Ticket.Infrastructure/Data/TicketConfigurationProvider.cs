using Concertable.Customer.Ticket.Infrastructure.Data.Configurations;
using Concertable.DataAccess.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Ticket.Infrastructure.Data;

internal sealed class TicketConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TicketEntityConfiguration());
    }
}
