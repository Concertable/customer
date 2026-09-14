using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.Ticket.Domain.Events;
using Concertable.Kernel.ValueObjects;

namespace Concertable.Customer.Ticket.UnitTests.Entities;

public sealed class TicketEntityTests
{
    private static readonly Guid TicketId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly byte[] QrCode = [1, 2, 3];
    private static readonly DateTime PurchaseDate = new(2026, 6, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateRange Period = new(
        new DateTime(2026, 7, 1, 19, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 7, 1, 23, 0, 0, DateTimeKind.Utc));

    private static TicketEntity NewPurchase() =>
        TicketEntity.Purchase(
            TicketId, UserId, 1, QrCode, PurchaseDate,
            "Concert", 25m, Period, 5, "Artist", 7, "Venue");

    [Fact]
    public void Purchase_SetsSnapshotFields()
    {
        var ticket = NewPurchase();

        Assert.Equal(TicketId, ticket.Id);
        Assert.Equal(UserId, ticket.UserId);
        Assert.Equal(1, ticket.ConcertId);
        Assert.Equal(QrCode, ticket.QrCode);
        Assert.Equal(PurchaseDate, ticket.PurchaseDate);
        Assert.Equal("Concert", ticket.ConcertName);
        Assert.Equal(25m, ticket.Price);
        Assert.Equal(Period, ticket.Period);
        Assert.Equal(5, ticket.ArtistId);
        Assert.Equal("Artist", ticket.ArtistName);
        Assert.Equal(7, ticket.VenueId);
        Assert.Equal("Venue", ticket.VenueName);
        Assert.False(ticket.HasReview);
    }

    [Fact]
    public void Purchase_RaisesTicketPurchasedDomainEvent()
    {
        var ticket = NewPurchase();

        var raised = Assert.IsType<TicketPurchasedDomainEvent>(Assert.Single(ticket.DomainEvents));
        Assert.Equal(TicketId, raised.TicketId);
        Assert.Equal(UserId, raised.UserId);
        Assert.Equal(1, raised.ConcertId);
        Assert.Equal(25m, raised.Price);
        Assert.Equal(PurchaseDate, raised.PurchaseDate);
    }

    [Fact]
    public void Create_DoesNotRaiseDomainEvents()
    {
        // The event-free factory is what the reflection-seed path uses — seeded historical
        // tickets must never publish TicketPurchasedEvent.
        var ticket = TicketEntity.Create(
            TicketId, UserId, 1, QrCode, PurchaseDate,
            "Concert", 25m, Period, 5, "Artist", 7, "Venue");

        Assert.Empty(ticket.DomainEvents);
    }

    [Fact]
    public void MarkReviewed_SetsHasReview()
    {
        var ticket = NewPurchase();

        ticket.MarkReviewed();

        Assert.True(ticket.HasReview);
    }

    [Fact]
    public void ClearDomainEvents_EmptiesDomainEvents()
    {
        var ticket = NewPurchase();

        ticket.ClearDomainEvents();

        Assert.Empty(ticket.DomainEvents);
    }
}
