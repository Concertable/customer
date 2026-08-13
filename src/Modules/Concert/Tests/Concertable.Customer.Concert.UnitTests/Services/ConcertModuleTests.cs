using Concertable.Customer.Concert.Application.Interfaces;
using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Infrastructure.Services;
using Concertable.Kernel.ValueObjects;
using Moq;

namespace Concertable.Customer.Concert.UnitTests.Services;

public sealed class ConcertModuleTests
{
    private readonly Mock<IConcertReadRepository> concertRepository = new();
    private readonly ConcertModule sut;

    public ConcertModuleTests()
    {
        this.sut = new ConcertModule(this.concertRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingConcert_ReturnsSomeAndForwardsCancellation()
    {
        var concert = NewConcert();
        using var cancellation = new CancellationTokenSource();
        this.concertRepository
            .Setup(repository => repository.GetDtoAsync(concert.Id, cancellation.Token))
            .ReturnsAsync(concert);

        var result = await this.sut.GetByIdAsync(concert.Id, cancellation.Token);

        Assert.True(result.TryGetValue(out var actual));
        Assert.Same(concert, actual);
        this.concertRepository.Verify(
            repository => repository.GetDtoAsync(concert.Id, cancellation.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_MissingConcert_ReturnsNone()
    {
        this.concertRepository
            .Setup(repository => repository.GetDtoAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertDto?)null);

        var result = await this.sut.GetByIdAsync(42);

        Assert.True(result.IsNone);
    }

    private static ConcertDto NewConcert() =>
        new(
            1,
            "Concert",
            25m,
            new DateRange(new DateTime(2030, 1, 1), new DateTime(2030, 1, 2)),
            new DateTime(2029, 12, 1),
            10,
            2,
            "Artist",
            3,
            "Venue",
            Guid.NewGuid(),
            Guid.NewGuid());
}
