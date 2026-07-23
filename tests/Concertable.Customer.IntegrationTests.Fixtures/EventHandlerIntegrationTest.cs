using Concertable.Kernel.DependencyInjection;
using Concertable.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Customer.IntegrationTests.Fixtures;

/// <summary>
/// Base for integration tests that exercise an <see cref="IIntegrationEventHandler{TEvent}"/> against a
/// real database. <see cref="DispatchAsync"/> runs the registered handler in its own DI scope, as the
/// message pipeline would. Derived classes resolve <see cref="IScoped{T}"/> for their own context to
/// seed and assert, so no test hand-rolls scope creation or service resolution.
/// </summary>
public abstract class EventHandlerIntegrationTest : IAsyncLifetime
{
    protected EventHandlerIntegrationTest(ApiFixture fixture, ITestOutputHelper output)
    {
        this.Fixture = fixture;
        fixture.AttachOutput(output);
    }

    protected ApiFixture Fixture { get; }

    /// <summary>Dispatches the event to its registered handler, in its own DI scope.</summary>
    protected Task DispatchAsync<TEvent>(TEvent @event, MessageEnvelope envelope)
        where TEvent : IIntegrationEvent =>
        Fixture.Services.GetRequiredService<IScoped<IIntegrationEventHandler<TEvent>>>()
            .RunAsync(handler => handler.HandleAsync(@event, envelope));

    public Task InitializeAsync() => Fixture.ResetAsync();

    public Task DisposeAsync()
    {
        Fixture.DetachOutput();
        return Task.CompletedTask;
    }
}
