using Concertable.Kernel.DependencyInjection;
using Concertable.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Customer.IntegrationTests.Fixtures;

/// <summary>
/// Base for integration tests that exercise an <see cref="IIntegrationEventHandler{TEvent}"/> against a
/// real database. <see cref="DispatchAsync"/> runs every registered handler for the event in a single DI
/// scope, exactly as the in-process message pipeline does. Derived classes resolve <see cref="IScoped{T}"/>
/// for their own context to seed and assert, so no test hand-rolls scope creation or service resolution.
/// </summary>
public abstract class EventHandlerIntegrationTest : IAsyncLifetime
{
    protected EventHandlerIntegrationTest(ApiFixture fixture, ITestOutputHelper output)
    {
        this.Fixture = fixture;
        fixture.AttachOutput(output);
    }

    protected ApiFixture Fixture { get; }

    /// <summary>Dispatches the event to every registered handler, in a single DI scope, as the pipeline does.</summary>
    protected Task DispatchAsync<TEvent>(TEvent @event, MessageEnvelope envelope)
        where TEvent : IIntegrationEvent =>
        Fixture.Services.GetRequiredService<IScoped<IEnumerable<IIntegrationEventHandler<TEvent>>>>()
            .RunAsync(async handlers =>
            {
                foreach (var handler in handlers)
                    await handler.HandleAsync(@event, envelope);
            });

    public Task InitializeAsync() => Fixture.ResetAsync();

    public Task DisposeAsync()
    {
        Fixture.DetachOutput();
        return Task.CompletedTask;
    }
}
