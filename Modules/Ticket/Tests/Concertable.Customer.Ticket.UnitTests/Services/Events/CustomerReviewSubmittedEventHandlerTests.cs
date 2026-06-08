using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Ticket.Application.Interfaces;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Services.Events;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Concertable.Customer.Ticket.UnitTests.Services.Events;

public sealed class CustomerReviewSubmittedEventHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static TicketDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<TicketDbContext>().UseInMemoryDatabase(dbName).Options,
            new TicketConfigurationProvider());

    private static TicketEntity NewTicket(Guid ticketId) =>
        TicketEntity.Create(
            ticketId, Guid.NewGuid(), 1, [1, 2, 3], Base.UtcDateTime,
            "Concert", 25m,
            new DateRange(Base.UtcDateTime.AddDays(-7), Base.UtcDateTime.AddDays(-6)),
            5, "Artist", 7, "Venue");

    private static CustomerReviewSubmittedEvent NewEvent(Guid ticketId) =>
        new(ticketId, 5, 7, 1, 4, "customer@test.com", "Great show");

    private static CustomerReviewSubmittedEventHandler NewSut(ITicketRepository repository, TicketDbContext context) =>
        new(repository, context, NullLogger<CustomerReviewSubmittedEventHandler>.Instance);

    [Fact]
    public async Task HandleAsync_MarksTicketReviewedAndRecordsInbox()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var ticketId = Guid.NewGuid();
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(Base);
        await using var context = NewContext(dbName);
        var ticket = NewTicket(ticketId);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();
        var repository = new Mock<ITicketRepository>();
        repository.Setup(r => r.GetByIdForReviewAsync(ticketId)).ReturnsAsync(ticket);

        // Act
        await NewSut(repository.Object, context).HandleAsync(NewEvent(ticketId), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var stored = await probe.Tickets.SingleAsync();
        Assert.True(stored.HasReview);
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(CustomerReviewSubmittedEventHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenTicketMissing_StillRecordsInbox()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(Base);
        await using var context = NewContext(dbName);
        var repository = new Mock<ITicketRepository>();
        repository.Setup(r => r.GetByIdForReviewAsync(It.IsAny<Guid>())).ReturnsAsync((TicketEntity?)null);

        // Act
        await NewSut(repository.Object, context).HandleAsync(NewEvent(Guid.NewGuid()), envelope);

        // Assert — the miss is logged and the message is consumed, not retried
        await using var probe = NewContext(dbName);
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(CustomerReviewSubmittedEventHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotTouchRepository()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(Base);
        await using var context = NewContext(dbName);
        context.AddInboxMessage(envelope, nameof(CustomerReviewSubmittedEventHandler));
        await context.SaveChangesAsync();
        var repository = new Mock<ITicketRepository>();

        // Act
        await NewSut(repository.Object, context).HandleAsync(NewEvent(Guid.NewGuid()), envelope);

        // Assert
        repository.Verify(r => r.GetByIdForReviewAsync(It.IsAny<Guid>()), Times.Never);
    }
}
