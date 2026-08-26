using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Customer.Venue.Application.Interfaces;
using Concertable.Customer.Venue.Infrastructure.Services;
using Moq;

namespace Concertable.Customer.Venue.UnitTests.Services;

public sealed class VenueServiceTests
{
    private readonly Mock<IVenueReadRepository> repository;
    private readonly VenueService sut;

    public VenueServiceTests()
    {
        this.repository = new Mock<IVenueReadRepository>();
        this.sut = new VenueService(repository.Object);
    }

    [Fact]
    public async Task GetDetailsByIdAsync_ExistingVenue_ReturnsSome()
    {
        var expected = NewVenueDetails();
        this.repository
            .Setup(repository => repository.GetDetailsByIdAsync(expected.Id))
            .ReturnsAsync(expected);

        var result = await this.sut.GetDetailsByIdAsync(expected.Id);

        Assert.True(result.TryGetValue(out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetDetailsByIdAsync_MissingVenue_ReturnsNone()
    {
        this.repository
            .Setup(repository => repository.GetDetailsByIdAsync(42))
            .ReturnsAsync((VenueDetails?)null);

        var result = await this.sut.GetDetailsByIdAsync(42);

        Assert.True(result.IsNone);
    }

    private static VenueDetails NewVenueDetails() => new(
        1,
        "The Forum",
        "Independent venue",
        "banner.jpg",
        "avatar.jpg",
        4.5,
        "Kent",
        "Tunbridge Wells",
        "venue@example.com",
        51.1,
        0.3);
}
