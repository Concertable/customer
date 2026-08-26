using Concertable.Contracts.Enums;
using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Application.Interfaces;
using Concertable.Customer.Artist.Infrastructure.Services;
using Moq;

namespace Concertable.Customer.Artist.UnitTests.Services;

public sealed class ArtistServiceTests
{
    private readonly Mock<IArtistReadRepository> repository;
    private readonly ArtistService sut;

    public ArtistServiceTests()
    {
        this.repository = new Mock<IArtistReadRepository>();
        this.sut = new ArtistService(repository.Object);
    }

    [Fact]
    public async Task GetDetailsByIdAsync_ExistingArtist_ReturnsSome()
    {
        var expected = NewArtistDetails();
        this.repository
            .Setup(repository => repository.GetDetailsByIdAsync(expected.Id))
            .ReturnsAsync(expected);

        var result = await this.sut.GetDetailsByIdAsync(expected.Id);

        Assert.True(result.TryGetValue(out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetDetailsByIdAsync_MissingArtist_ReturnsNone()
    {
        this.repository
            .Setup(repository => repository.GetDetailsByIdAsync(42))
            .ReturnsAsync((ArtistDetails?)null);

        var result = await this.sut.GetDetailsByIdAsync(42);

        Assert.True(result.IsNone);
    }

    private static ArtistDetails NewArtistDetails() => new(
        1,
        "The Comets",
        "Independent artist",
        "banner.jpg",
        "avatar.jpg",
        4.5,
        [Genre.Rock],
        "artist@example.com",
        "Kent",
        "Tunbridge Wells",
        51.1,
        0.3);
}
