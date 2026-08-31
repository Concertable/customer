using Concertable.Customer.Review.Contracts.Events;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Seed.Simulator;

internal sealed class SmokeBusTransport : IBusTransport
{
    public Task PublishAsync<TEvent>(
        TEvent @event,
        MessageEnvelope envelope,
        CancellationToken ct = default)
        where TEvent : IIntegrationEvent =>
        @event is CustomerReviewSubmittedEvent
            ? Task.CompletedTask
            : Task.FromException(new InvalidOperationException($"Unexpected seed event '{typeof(TEvent).Name}'."));

    public Task SendAsync<TCommand>(
        TCommand command,
        MessageEnvelope envelope,
        CancellationToken ct = default)
        where TCommand : IIntegrationCommand =>
        Task.FromException(new InvalidOperationException("The Customer seed simulator does not send commands."));
}
