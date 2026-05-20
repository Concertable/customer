using Concertable.Customer.Profile.Infrastructure.Data;
using Concertable.Messaging.Domain;
using Concertable.User.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Profile.Infrastructure.Events;

internal class CustomerProfileCreationHandler : IIntegrationEventHandler<CustomerRegisteredEvent>
{
    private readonly ProfileDbContext context;

    public CustomerProfileCreationHandler(ProfileDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(CustomerRegisteredEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(CustomerProfileCreationHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(CustomerProfileCreationHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        context.CustomerProfiles.Add(new CustomerProfileEntity(e.UserId));
        await context.SaveChangesAsync(ct);
    }
}
