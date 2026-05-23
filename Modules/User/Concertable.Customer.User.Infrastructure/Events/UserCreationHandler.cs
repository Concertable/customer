using Concertable.Auth.Contracts;
using Concertable.Auth.Contracts.Events;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.User.Infrastructure.Events;

internal class UserCreationHandler : IIntegrationEventHandler<CredentialRegisteredEvent>
{
    private readonly UserDbContext context;

    public UserCreationHandler(UserDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(CredentialRegisteredEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (e.ClientId is not ClientIds.CustomerWeb and not ClientIds.CustomerMobile)
            return;

        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(UserCreationHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(UserCreationHandler));

        context.Users.Add(UserEntity.FromRegistration(e.UserId, e.Email));
        await context.SaveChangesAsync(ct);
    }
}
