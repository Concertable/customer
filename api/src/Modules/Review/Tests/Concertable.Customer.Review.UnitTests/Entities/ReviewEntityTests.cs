using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Review.Domain.Events;
using Reunion;
using Reunion.Errors;

namespace Concertable.Customer.Review.UnitTests.Entities;

public sealed class ReviewEntityTests
{
    private static readonly Guid TicketId = Guid.NewGuid();

    private static Result<ReviewEntity, ValidationErrors> NewReview(byte stars = 4) =>
        ReviewEntity.Create(TicketId, stars, "Great show", "customer@test.com", 5, 7, 1);

    [Fact]
    public void Create_ValidDetails_ReturnsReview()
    {
        var result = NewReview(stars: 4);

        Assert.True(result.TryGetValue(out var review));
        Assert.Equal(TicketId, review.TicketId);
        Assert.Equal(4, review.Stars);
        Assert.Equal("Great show", review.Details);
        Assert.Equal("customer@test.com", review.Email);
        Assert.Equal(5, review.ArtistId);
        Assert.Equal(7, review.VenueId);
        Assert.Equal(1, review.ConcertId);
    }

    [Fact]
    public void Create_ValidDetails_RaisesReviewCreatedDomainEvent()
    {
        var result = NewReview(stars: 4);
        var review = GetReview(result);

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
    public void Create_BoundaryStars_ReturnsReview(byte stars)
    {
        var result = NewReview(stars);

        Assert.True(result.TryGetValue(out var review));
        Assert.Equal(stars, review.Stars);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_StarsOutOfRange_ReturnsStructuredValidation(byte stars)
    {
        var result = NewReview(stars);

        Assert.True(result.TryGetError(out var errors));
        Assert.Equal(["Stars must be between 1 and 5."], errors.Errors["Stars"]);
    }

    [Fact]
    public void ClearDomainEvents_RaisedEvent_EmptiesDomainEvents()
    {
        var result = NewReview();
        var review = GetReview(result);

        review.ClearDomainEvents();

        Assert.Empty(review.DomainEvents);
    }

    private static ReviewEntity GetReview(Result<ReviewEntity, ValidationErrors> result)
    {
        Assert.True(result.TryGetValue(out var review));
        return review;
    }
}
