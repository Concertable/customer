using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Review.Domain.Events;
using Concertable.Kernel;

namespace Concertable.Customer.Review.UnitTests.Entities;

public sealed class ReviewEntityTests
{
    private static readonly Guid TicketId = Guid.NewGuid();

    private static ReviewEntity NewReview(byte stars = 4) =>
        ReviewEntity.Create(TicketId, stars, "Great show", "customer@test.com", 5, 7, 1);

    [Fact]
    public void Create_SetsReviewDetails()
    {
        var review = NewReview(stars: 4);

        Assert.Equal(TicketId, review.TicketId);
        Assert.Equal(4, review.Stars);
        Assert.Equal("Great show", review.Details);
        Assert.Equal("customer@test.com", review.Email);
        Assert.Equal(5, review.ArtistId);
        Assert.Equal(7, review.VenueId);
        Assert.Equal(1, review.ConcertId);
    }

    [Fact]
    public void Create_RaisesReviewCreatedDomainEvent()
    {
        var review = NewReview(stars: 4);

        var raised = Assert.IsType<ReviewCreatedDomainEvent>(Assert.Single(review.DomainEvents));
        Assert.Equal(TicketId, raised.TicketId);
        Assert.Equal(5, raised.ArtistId);
        Assert.Equal(7, raised.VenueId);
        Assert.Equal(1, raised.ConcertId);
        Assert.Equal(4, raised.Stars);
        Assert.Equal("customer@test.com", raised.Email);
        Assert.Equal("Great show", raised.Details);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Create_WithBoundaryStars_Succeeds(byte stars)
    {
        var review = NewReview(stars);

        Assert.Equal(stars, review.Stars);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_WithStarsOutOfRange_Throws(byte stars)
    {
        Assert.Throws<DomainException>(() => NewReview(stars));
    }

    [Fact]
    public void ClearDomainEvents_EmptiesDomainEvents()
    {
        var review = NewReview();

        review.ClearDomainEvents();

        Assert.Empty(review.DomainEvents);
    }
}
