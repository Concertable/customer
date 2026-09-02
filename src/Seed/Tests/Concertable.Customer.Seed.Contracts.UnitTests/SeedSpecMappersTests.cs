using Concertable.Customer.Seed.Contracts;
using Concertable.Customer.Review.Contracts.Events;
using Concertable.Messaging.Contracts;
using Concertable.Seed.Identity;

namespace Concertable.Customer.Seed.Contracts.UnitTests;

public sealed class SeedSpecMappersTests
{
    [Fact]
    public void ToSubmittedEvent_ConfirmedReview_PreservesCanonicalWireData()
    {
        var fixture = new DevFixture();

        var result = fixture.ConfirmedConcertReview.ToSubmittedEvent();

        Assert.Equal(new Guid("d0000000-0000-0000-0000-000000000002"), result.TicketId);
        Assert.Equal(1, result.ArtistId);
        Assert.Equal(1, result.VenueId);
        Assert.Equal(12, result.ConcertId);
        Assert.Equal(5, result.Stars);
        Assert.Equal(SeedCustomers.CustomerEmail(1), result.Email);
        Assert.Equal("Great show", result.Details);
        Assert.Single(fixture.Reviews);
    }

    [Fact]
    public void ToEnvelope_RepeatedFixtureConstruction_ReturnsSameInboxIdentity()
    {
        var firstFixture = new DevFixture();
        var secondFixture = new DevFixture();

        var first = firstFixture.ConfirmedConcertReview.ToEnvelope();
        var second = secondFixture.ConfirmedConcertReview.ToEnvelope();

        Assert.Equal(first, second);
        Assert.Equal(new Guid("e0000000-0000-0000-0000-000000000001"), first.MessageId);
        Assert.Equal(MessageTypeAttribute.Resolve(typeof(CustomerReviewSubmittedEvent)), first.MessageType);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero), first.OccurredAtUtc);
    }
}
