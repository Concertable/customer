using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Kernel.DependencyInjection;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Concert.IntegrationTests;

[Collection("Integration")]
public sealed class ConcertRatingProjectionHandlerTests(ApiFixture fixture, ITestOutputHelper output)
    : EventHandlerIntegrationTest(fixture, output)
{
    private const int ConcertId = 90002;
    private const int UnknownConcertId = 90999;
    private readonly IScoped<ConcertDbContext> scoped = fixture.Services.GetRequiredService<IScoped<ConcertDbContext>>();

    private Task SeedConcertAsync() =>
        DispatchAsync(ConcertChangedEvents.Create(ConcertId), MessageEnvelope.Create<ConcertChangedEvent>(TestTime.Now));

    [Fact]
    public async Task HandleAsync_UpdatesRatingAndRecordsInbox()
    {
        await SeedConcertAsync();
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(TestTime.Now);

        await DispatchAsync(new ConcertRatingUpdatedEvent(ConcertId, 4.5, 12), envelope);

        var concert = await scoped.RunAsync(ctx => ctx.Concerts.SingleAsync(c => c.Id == ConcertId));
        Assert.Equal(4.5, concert.AverageRating);
        Assert.Equal(12, concert.ReviewCount);
        Assert.True(await scoped.RunAsync(ctx => ctx.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertRatingProjectionHandler))));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotUpdate()
    {
        await SeedConcertAsync();
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(TestTime.Now);
        await scoped.RunAsync(async ctx =>
        {
            ctx.AddInboxMessage(envelope, nameof(ConcertRatingProjectionHandler));
            await ctx.SaveChangesAsync();
        });

        await DispatchAsync(new ConcertRatingUpdatedEvent(ConcertId, 4.5, 12), envelope);

        var concert = await scoped.RunAsync(ctx => ctx.Concerts.SingleAsync(c => c.Id == ConcertId));
        Assert.Equal(0, concert.AverageRating);
        Assert.Equal(0, concert.ReviewCount);
    }

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_PersistsNothing()
    {
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(TestTime.Now);

        await DispatchAsync(new ConcertRatingUpdatedEvent(UnknownConcertId, 4.5, 12), envelope);

        // the early return skips the save, so the inbox row is not consumed and a redelivery can retry
        Assert.False(await scoped.RunAsync(ctx => ctx.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertRatingProjectionHandler))));
    }
}
