using Concertable.Customer.User.Domain.Events;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Contracts.Events;

namespace Concertable.Customer.User.Infrastructure.Events;

/// <summary>
/// Publishes <see cref="PaymentMethodOwnerRegisteredEvent"/> when a customer is created — fires for both the
/// registration path and the dev/E2E seeder, since both go through <c>UserEntity.FromRegistration</c>, so
/// Payment provisions a Stripe customer keyed on the user id. Without it a ticket purchase is refused: the
/// payment session requires the payer to already carry a provider customer.
/// </summary>
internal sealed class UserRegisteredDomainEventHandler : IPreCommitDomainEventHandler<UserRegisteredDomainEvent>
{
    private readonly IBus bus;

    public UserRegisteredDomainEventHandler(IBus bus)
    {
        this.bus = bus;
    }

    public Task HandleAsync(UserRegisteredDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new PaymentMethodOwnerRegisteredEvent(e.UserId, e.Email), ct);
}
