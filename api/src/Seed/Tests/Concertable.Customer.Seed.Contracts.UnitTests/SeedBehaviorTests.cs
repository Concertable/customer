using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Seed.Contracts;
using Concertable.Customer.Seed.Infrastructure.Factories;
using Concertable.Customer.Seed.Simulator;
using Concertable.Messaging.Contracts;
using Microsoft.Extensions.Hosting;

namespace Concertable.Customer.Seed.Contracts.UnitTests;

public sealed class SeedBehaviorTests
{
    [Fact]
    public void ReviewFactory_AndSubmittedEvent_PreserveTheSameFixtureData()
    {
        var spec = new DevFixture().ConfirmedConcertReview;

        var entity = ReviewFactory.Create(spec);
        var @event = spec.ToSubmittedEvent();

        Assert.Equal(@event.TicketId, entity.TicketId);
        Assert.Equal(@event.ArtistId, entity.ArtistId);
        Assert.Equal(@event.VenueId, entity.VenueId);
        Assert.Equal(@event.ConcertId, entity.ConcertId);
        Assert.Equal(@event.Stars, entity.Stars);
        Assert.Equal(@event.Email, entity.Email);
        Assert.Equal(@event.Details, entity.Details);
    }

    [Fact]
    public async Task SeedEventPublisher_RepeatedRuns_PublishOneIdenticalEventAndStop()
    {
        var first = await RunPublisherAsync();
        var second = await RunPublisherAsync();

        Assert.True(first.StopRequested);
        Assert.True(second.StopRequested);
        Assert.Equal(first.Event, second.Event);
        Assert.Equal(first.Envelope, second.Envelope);
    }

    private static async Task<PublisherRun> RunPublisherAsync()
    {
        var transport = new RecordingBusTransport();
        var lifetime = new RecordingHostApplicationLifetime();
        var publisher = new SeedEventPublisher(transport, new DevFixture(), lifetime);

        await publisher.StartAsync(CancellationToken.None).ConfigureAwait(false);
        await lifetime.WaitForStopAsync().ConfigureAwait(false);

        var published = Assert.Single(transport.Published);
        return new(
            Assert.IsType<CustomerReviewSubmittedEvent>(published.Event),
            published.Envelope,
            lifetime.StopRequested);
    }

    private sealed class RecordingBusTransport : IBusTransport
    {
        public List<(IIntegrationEvent Event, MessageEnvelope Envelope)> Published { get; } = [];

        public Task PublishAsync<TEvent>(
            TEvent @event,
            MessageEnvelope envelope,
            CancellationToken ct = default)
            where TEvent : IIntegrationEvent
        {
            Published.Add((@event, envelope));
            return Task.CompletedTask;
        }

        public Task SendAsync<TCommand>(
            TCommand command,
            MessageEnvelope envelope,
            CancellationToken ct = default)
            where TCommand : IIntegrationCommand =>
            throw new NotSupportedException();
    }

    private sealed class RecordingHostApplicationLifetime : IHostApplicationLifetime
    {
        private readonly TaskCompletionSource stopped =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;
        public bool StopRequested { get; private set; }

        public void StopApplication()
        {
            StopRequested = true;
            stopped.SetResult();
        }

        public Task WaitForStopAsync() => stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private sealed record PublisherRun(
        CustomerReviewSubmittedEvent Event,
        MessageEnvelope Envelope,
        bool StopRequested);
}
