using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Kernel;

namespace Concertable.Customer.Concert.UnitTests.Entities;

public sealed class ConcertEntityTests
{
    private static readonly Guid PayeeUserId = Guid.NewGuid();
    private static readonly Guid PayeeOwnerId = Guid.NewGuid();

    private static ConcertEntity NewConcert(int totalTickets = 10) =>
        ConcertEntity.Create(
            1,
            "Concert",
            "About",
            "banner.png",
            "avatar.png",
            totalTickets,
            25m,
            new DateRange(
                new DateTime(2026, 7, 1, 19, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 1, 23, 0, 0, DateTimeKind.Utc)),
            new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            5,
            "Artist",
            7,
            "Venue",
            PayeeUserId,
            PayeeOwnerId);

    [Fact]
    public void Create_SetsAvailableTicketsToTotal()
    {
        var concert = NewConcert(totalTickets: 10);

        Assert.Equal(10, concert.TotalTickets);
        Assert.Equal(10, concert.AvailableTickets);
    }

    [Fact]
    public void DecrementAvailability_ReducesAvailableTickets()
    {
        var concert = NewConcert(totalTickets: 10);

        concert.DecrementAvailability(3);

        Assert.Equal(7, concert.AvailableTickets);
        Assert.Equal(10, concert.TotalTickets);
    }

    [Fact]
    public void DecrementAvailability_ToExactlyZero_Succeeds()
    {
        var concert = NewConcert(totalTickets: 10);

        concert.DecrementAvailability(10);

        Assert.Equal(0, concert.AvailableTickets);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecrementAvailability_WithNonPositiveQuantity_Throws(int quantity)
    {
        var concert = NewConcert();

        Assert.Throws<DomainException>(() => concert.DecrementAvailability(quantity));
    }

    [Fact]
    public void DecrementAvailability_BeyondAvailable_Throws()
    {
        var concert = NewConcert(totalTickets: 10);

        Assert.Throws<DomainException>(() => concert.DecrementAvailability(11));
    }

    [Fact]
    public void RestoreAvailability_IncreasesAvailableTickets()
    {
        var concert = NewConcert(totalTickets: 10);
        concert.DecrementAvailability(3);

        concert.RestoreAvailability(2);

        Assert.Equal(9, concert.AvailableTickets);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RestoreAvailability_WithNonPositiveQuantity_Throws(int quantity)
    {
        var concert = NewConcert();
        concert.DecrementAvailability(1);

        Assert.Throws<DomainException>(() => concert.RestoreAvailability(quantity));
    }

    [Fact]
    public void RestoreAvailability_BeyondCapacity_Throws()
    {
        var concert = NewConcert(totalTickets: 10);

        Assert.Throws<DomainException>(() => concert.RestoreAvailability(1));
    }

    [Fact]
    public void Update_PreservesSoldCount()
    {
        var concert = NewConcert(totalTickets: 10);
        concert.DecrementAvailability(3);

        concert.Update(
            "Renamed", "About", "banner.png", "avatar.png",
            20, 30m, concert.Period, concert.DatePosted,
            5, "Artist", 7, "Venue", PayeeUserId, PayeeOwnerId);

        Assert.Equal(20, concert.TotalTickets);
        Assert.Equal(17, concert.AvailableTickets);
        Assert.Equal("Renamed", concert.Name);
    }

    [Fact]
    public void Update_WhenNewTotalBelowSold_ClampsAvailableToZero()
    {
        var concert = NewConcert(totalTickets: 10);
        concert.DecrementAvailability(3);

        concert.Update(
            "Concert", "About", "banner.png", "avatar.png",
            2, 25m, concert.Period, concert.DatePosted,
            5, "Artist", 7, "Venue", PayeeUserId, PayeeOwnerId);

        Assert.Equal(2, concert.TotalTickets);
        Assert.Equal(0, concert.AvailableTickets);
    }

    [Fact]
    public void UpdateRating_SetsRatingAndCount()
    {
        var concert = NewConcert();

        concert.UpdateRating(4.5, 12);

        Assert.Equal(4.5, concert.AverageRating);
        Assert.Equal(12, concert.ReviewCount);
    }
}
