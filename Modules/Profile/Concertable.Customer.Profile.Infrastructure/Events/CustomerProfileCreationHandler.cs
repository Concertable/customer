using Concertable.Customer.Profile.Infrastructure.Data;
using Concertable.User.Contracts.Events;

namespace Concertable.Customer.Profile.Infrastructure.Events;

internal class CustomerProfileCreationHandler : IIntegrationEventHandler<UserRegisteredEvent>
{
    private readonly ProfileDbContext context;

    public CustomerProfileCreationHandler(ProfileDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(UserRegisteredEvent e, CancellationToken ct = default)
    {
        if (e.Role != Role.Customer)
            return;

        context.CustomerProfiles.Add(new CustomerProfileEntity(e.UserId));
        await context.SaveChangesAsync(ct);
    }
}
