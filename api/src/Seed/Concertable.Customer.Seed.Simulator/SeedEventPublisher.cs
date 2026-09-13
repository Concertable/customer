using Concertable.Customer.Seed.Contracts;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Seed.Simulator;

internal sealed class SeedEventPublisher : BackgroundService
{
    private readonly IBusTransport transport;
    private readonly DevFixture fixture;
    private readonly IHostApplicationLifetime lifetime;

    public SeedEventPublisher(
        IBusTransport transport,
        DevFixture fixture,
        IHostApplicationLifetime lifetime)
    {
        this.transport = transport;
        this.fixture = fixture;
        this.lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var review in this.fixture.Reviews)
        {
            await this.transport.PublishAsync(
                review.ToSubmittedEvent(),
                review.ToEnvelope(),
                stoppingToken).ConfigureAwait(false);
        }

        this.lifetime.StopApplication();
    }
}
