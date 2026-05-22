using Concertable.Auth.Contracts;
using Concertable.Auth.Contracts.Events;
using Concertable.Customer.Profile.Infrastructure.Data;
using Concertable.Messaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Profile.Infrastructure.Events;

internal class CustomerProfileCreationHandler : IIntegrationEventHandler<CredentialRegisteredEvent>
{
    private readonly ProfileDbContext context;

    public CustomerProfileCreationHandler(ProfileDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(CredentialRegisteredEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (e.ClientId is not ClientIds.CustomerWeb and not ClientIds.CustomerMobile)
            return;

        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(CustomerProfileCreationHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(CustomerProfileCreationHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        context.CustomerProfiles.Add(new CustomerProfileEntity(e.UserId));
        await context.SaveChangesAsync(ct);
    }
}
