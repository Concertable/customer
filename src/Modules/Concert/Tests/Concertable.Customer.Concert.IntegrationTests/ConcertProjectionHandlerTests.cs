using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Contracts.Enums;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Kernel.DependencyInjection;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Concert.IntegrationTests;

[Collection("Integration")]
public sealed class ConcertProjectionHandlerTests(ApiFixture fixture, ITestOutputHelper output)
    : EventHandlerIntegrationTest(fixture, output)
{
    private const int ConcertId = 90001;
    private readonly IScoped<ConcertDbContext> scoped = fixture.Services.GetRequiredService<IScoped<ConcertDbContext>>();

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_CreatesProjection()
    {
        var envelope = MessageEnvelope.Create<ConcertChangedEvent>(TestTime.Now);
        var e = ConcertChangedEvents.Create(ConcertId, genres: [Genre.Rock, Genre.Pop]);

        await DispatchAsync(e, envelope);

        var concert = await scoped.RunAsync(ctx => ctx.Concerts.Include(c => c.Genres).SingleAsync(c => c.Id == ConcertId));
        Assert.Equal(e.ConcertId, concert.Id);
        Assert.Equal(e.Name, concert.Name);
        Assert.Equal(e.About, concert.About);
        Assert.Equal(e.BannerUrl, concert.BannerUrl);
        Assert.Equal(e.Avatar, concert.Avatar);
        Assert.Equal(10, concert.TotalTickets);
        Assert.Equal(10, concert.AvailableTickets);
        Assert.Equal(e.Price, concert.Price);
        Assert.Equal(e.Period, concert.Period);
        Assert.Equal(e.ArtistId, concert.ArtistId);
        Assert.Equal(e.ArtistName, concert.ArtistName);
        Assert.Equal(e.VenueId, concert.VenueId);
        Assert.Equal(e.VenueName, concert.VenueName);
        Assert.Equal(e.PayeeUserId, concert.PayeeUserId);
        Assert.Equal(e.PayeeOwnerId, concert.PayeeOwnerId);
        Assert.Equal([Genre.Rock, Genre.Pop], concert.Genres.Select(g => g.Genre).Order());
        Assert.True(await scoped.RunAsync(ctx => ctx.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertProjectionHandler))));
    }

    [Fact]
    public async Task HandleAsync_WhenConcertExists_UpdatesPreservingSoldAndSyncsGenres()
    {
        // 3 of 10 already sold; genres start as Rock+Pop
        await DispatchAsync(
            ConcertChangedEvents.Create(ConcertId, genres: [Genre.Rock, Genre.Pop]),
            MessageEnvelope.Create<ConcertChangedEvent>(TestTime.Now));
        await scoped.RunAsync(async ctx =>
        {
            var seeded = await ctx.Concerts.SingleAsync(c => c.Id == ConcertId);
            seeded.DecrementAvailability(3);
            await ctx.SaveChangesAsync();
        });

        await DispatchAsync(
            ConcertChangedEvents.Create(ConcertId, name: "Renamed", totalTickets: 20, genres: [Genre.Pop, Genre.Jazz]),
            MessageEnvelope.Create<ConcertChangedEvent>(TestTime.Now));

        var concert = await scoped.RunAsync(ctx => ctx.Concerts.Include(c => c.Genres).SingleAsync(c => c.Id == ConcertId));
        Assert.Equal("Renamed", concert.Name);
        Assert.Equal(20, concert.TotalTickets);
        Assert.Equal(17, concert.AvailableTickets);
        Assert.Equal([Genre.Pop, Genre.Jazz], concert.Genres.Select(g => g.Genre).Order());
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotApplyChanges()
    {
        var envelope = MessageEnvelope.Create<ConcertChangedEvent>(TestTime.Now);
        await DispatchAsync(ConcertChangedEvents.Create(ConcertId, name: "Original"), MessageEnvelope.Create<ConcertChangedEvent>(TestTime.Now));
        await scoped.RunAsync(async ctx =>
        {
            ctx.AddInboxMessage(envelope, nameof(ConcertProjectionHandler));
            await ctx.SaveChangesAsync();
        });

        await DispatchAsync(ConcertChangedEvents.Create(ConcertId, name: "Renamed"), envelope);

        var concert = await scoped.RunAsync(ctx => ctx.Concerts.SingleAsync(c => c.Id == ConcertId));
        Assert.Equal("Original", concert.Name);
    }
}
