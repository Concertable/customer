using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Review.Domain.Events;
using Concertable.Customer.Review.Infrastructure.Events;
using Concertable.Messaging.Contracts;
using Moq;

namespace Concertable.Customer.Review.UnitTests.Events;

public sealed class ReviewCreatedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_PublishesCustomerReviewSubmittedEventFromDomainEvent()
    {
        // Arrange
        var bus = new Mock<IBus>();
        CustomerReviewSubmittedEvent? published = null;
        bus.Setup(b => b.PublishAsync(It.IsAny<CustomerReviewSubmittedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<CustomerReviewSubmittedEvent, CancellationToken>((e, _) => published = e)
            .Returns(Task.CompletedTask);
        var sut = new ReviewCreatedDomainEventHandler(bus.Object);
        var domainEvent = new ReviewCreatedDomainEvent(
            Guid.NewGuid(), 5, 7, 1, 4, "customer@test.com", "Great show");

        // Act
        await sut.HandleAsync(domainEvent);

        // Assert
        Assert.NotNull(published);
        Assert.Equal(domainEvent.TicketId, published!.TicketId);
        Assert.Equal(domainEvent.ArtistId, published.ArtistId);
        Assert.Equal(domainEvent.VenueId, published.VenueId);
        Assert.Equal(domainEvent.ConcertId, published.ConcertId);
        Assert.Equal(domainEvent.Stars, published.Stars);
        Assert.Equal(domainEvent.Email, published.Email);
        Assert.Equal(domainEvent.Details, published.Details);
    }
}
