using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Customer.Ticket.Domain.Events;
using Concertable.Customer.Ticket.Infrastructure.Events;
using Concertable.Messaging.Contracts;
using Moq;

namespace Concertable.Customer.Ticket.UnitTests.Events;

public sealed class TicketPurchasedDomainEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_PublishesTicketPurchasedEventFromDomainEvent()
    {
        // Arrange
        var bus = new Mock<IBus>();
        TicketPurchasedEvent? published = null;
        bus.Setup(b => b.PublishAsync(It.IsAny<TicketPurchasedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TicketPurchasedEvent, CancellationToken>((e, _) => published = e)
            .Returns(Task.CompletedTask);
        var sut = new TicketPurchasedDomainEventHandler(bus.Object);
        var domainEvent = new TicketPurchasedDomainEvent(
            Guid.NewGuid(), Guid.NewGuid(), 1, 25m, new DateTime(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc));

        // Act
        await sut.HandleAsync(domainEvent);

        // Assert
        Assert.NotNull(published);
        Assert.Equal(domainEvent.TicketId, published!.TicketId);
        Assert.Equal(domainEvent.UserId, published.UserId);
        Assert.Equal(domainEvent.ConcertId, published.ConcertId);
        Assert.Equal(domainEvent.Price, published.Price);
        Assert.Equal(domainEvent.PurchaseDate, published.PurchaseDate);
    }
}
