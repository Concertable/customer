using Concertable.Customer.Profile.Infrastructure.Data;
using Concertable.User.Contracts.Events;

namespace Concertable.Customer.Profile.Infrastructure.Events;

internal class CustomerProfileCreationHandler : IIntegrationEventHandler<CustomerRegisteredEvent>
{
    private readonly ProfileDbContext context;

    public CustomerProfileCreationHandler(ProfileDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(CustomerRegisteredEvent e, CancellationToken ct = default)
    {
        context.CustomerProfiles.Add(new CustomerProfileEntity(e.UserId));
        await context.SaveChangesAsync(ct);
    }
}
