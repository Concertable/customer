using Concertable.Contracts.Enums;
using Concertable.Customer.Preference.Application.Errors;
using Concertable.Customer.Preference.Application.Interfaces;
using Concertable.Customer.Preference.Application.Requests;
using Concertable.Customer.Preference.Domain.Entities;
using Concertable.Customer.Preference.Infrastructure.Services;
using Concertable.Customer.User.Contracts;
using Concertable.Kernel.Functional;
using Concertable.Kernel.Geometry;
using Concertable.Kernel.Identity;
using Moq;

namespace Concertable.Customer.Preference.UnitTests.Services;

public sealed class PreferenceServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IPreferenceRepository> preferenceRepository;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly Mock<IUserModule> userModule;
    private readonly Mock<IGeometryCalculator> geometryCalculator;
    private readonly PreferenceService sut;

    public PreferenceServiceTests()
    {
        this.preferenceRepository = new Mock<IPreferenceRepository>();
        this.currentUser = new Mock<ICurrentUser>();
        this.userModule = new Mock<IUserModule>();
        this.geometryCalculator = new Mock<IGeometryCalculator>();
        this.currentUser.SetupGet(user => user.Id).Returns(UserId);
        this.preferenceRepository
            .Setup(repository => repository.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PreferenceEntity?)null);
        this.preferenceRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<PreferenceEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PreferenceEntity preference, CancellationToken _) => preference);
        this.preferenceRepository
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        this.sut = new PreferenceService(
            preferenceRepository.Object,
            currentUser.Object,
            userModule.Object,
            geometryCalculator.Object);
    }

    #region GetByUserIdAsync

    [Fact]
    public async Task GetByUserIdAsync_ExistingPreference_ReturnsSome()
    {
        var preference = NewPreference(UserId);
        this.preferenceRepository
            .Setup(repository => repository.GetByUserIdAsync(UserId))
            .ReturnsAsync(preference);

        var result = await this.sut.GetByUserIdAsync(UserId);

        Assert.True(result.TryGetValue(out var dto));
        Assert.Equal(UserId, dto.UserId);
        Assert.Equal(20, dto.RadiusKm);
        Assert.Contains(Genre.Rock, dto.Genres);
    }

    [Fact]
    public async Task GetByUserIdAsync_MissingPreference_ReturnsNone()
    {
        var result = await this.sut.GetByUserIdAsync(UserId);

        Assert.True(result.IsNone);
    }

    [Fact]
    public async Task GetByUserAsync_CurrentUser_ReturnsTheirPreference()
    {
        this.preferenceRepository
            .Setup(repository => repository.GetByUserIdAsync(UserId))
            .ReturnsAsync(NewPreference(UserId));

        var result = await this.sut.GetByUserAsync();

        Assert.True(result.TryGetValue(out var dto));
        Assert.Equal(UserId, dto.UserId);
    }

    #endregion

    #region GetAsync

    [Fact]
    public async Task GetAsync_RepositoryResults_ReturnsMaterializedReadOnlyList()
    {
        var source = new List<PreferenceEntity> { NewPreference(UserId) };
        this.preferenceRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(source);

        var result = await this.sut.GetAsync();
        source.Add(NewPreference(Guid.NewGuid()));

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAsync_NoPreferences_ReturnsEmptyList()
    {
        this.preferenceRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await this.sut.GetAsync();

        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ExistingPreference_ReturnsConflictWithoutPersisting()
    {
        this.preferenceRepository
            .Setup(repository => repository.GetByUserIdAsync(UserId))
            .ReturnsAsync(NewPreference(UserId));

        var result = await this.sut.CreateAsync(NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<CreatePreferenceError.PreferenceAlreadyExists>(error);
        this.preferenceRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<PreferenceEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        this.preferenceRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NewPreference_PersistsAndReturnsPreference()
    {
        var requestedUserId = Guid.NewGuid();

        var result = await this.sut.CreateAsync(NewRequest(), requestedUserId);

        Assert.True(result.TryGetValue(out var dto));
        Assert.Equal(requestedUserId, dto.UserId);
        Assert.Equal(30, dto.RadiusKm);
        Assert.Equal([Genre.Rock, Genre.Jazz], dto.Genres.Order());
        this.preferenceRepository.Verify(
            repository => repository.AddAsync(
                It.Is<PreferenceEntity>(preference =>
                    preference.UserId == requestedUserId
                    && preference.RadiusKm == 30),
                It.IsAny<CancellationToken>()),
            Times.Once);
        this.preferenceRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AddFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.preferenceRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<PreferenceEntity>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.CreateAsync(NewRequest()));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task CreateAsync_CancelledSave_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        this.preferenceRepository
            .Setup(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromCanceled(cancellation.Token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.sut.CreateAsync(NewRequest()));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_MissingPreference_ReturnsNotFound()
    {
        this.preferenceRepository
            .Setup(repository => repository.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PreferenceEntity?)null);

        var result = await this.sut.UpdateAsync(42, NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<UpdatePreferenceError.PreferenceNotFound>(error);
        this.preferenceRepository.Verify(
            repository => repository.Update(It.IsAny<PreferenceEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ForeignPreference_ReturnsForbidden()
    {
        this.preferenceRepository
            .Setup(repository => repository.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPreference(Guid.NewGuid()));

        var result = await this.sut.UpdateAsync(42, NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<UpdatePreferenceError.PreferenceNotOwned>(error);
        this.preferenceRepository.Verify(
            repository => repository.Update(It.IsAny<PreferenceEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OwnPreference_ReturnsTrackedUpdatedPreference()
    {
        var preference = NewPreference(UserId);
        this.preferenceRepository
            .Setup(repository => repository.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);

        var result = await this.sut.UpdateAsync(42, NewRequest());

        Assert.True(result.TryGetValue(out var dto));
        Assert.Equal(30, dto.RadiusKm);
        Assert.Equal([Genre.Rock, Genre.Jazz], dto.Genres.Order());
        this.preferenceRepository.Verify(
            repository => repository.Update(preference),
            Times.Once);
        this.preferenceRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        this.preferenceRepository.Verify(
            repository => repository.GetByIdAsync(42, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RepositoryFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.preferenceRepository
            .Setup(repository => repository.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.UpdateAsync(42, NewRequest()));

        Assert.Same(expected, actual);
    }

    #endregion

    #region GetUserIdsByLocationAndGenresAsync

    [Fact]
    public async Task GetUserIdsByLocationAndGenresAsync_NoPreferences_ReturnsEmptyList()
    {
        this.preferenceRepository
            .Setup(repository => repository.GetByMatchingGenresAsync(It.IsAny<IEnumerable<Genre>>()))
            .ReturnsAsync([]);

        var result = await this.sut.GetUserIdsByLocationAndGenresAsync(
            51,
            -1,
            [Genre.Rock]);

        Assert.Empty(result);
        this.userModule.Verify(
            module => module.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserIdsByLocationAndGenresAsync_MatchingUserWithinRadius_ReturnsUserId()
    {
        var preference = NewPreference(UserId);
        this.preferenceRepository
            .Setup(repository => repository.GetByMatchingGenresAsync(It.IsAny<IEnumerable<Genre>>()))
            .ReturnsAsync([preference]);
        this.userModule
            .Setup(module => module.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([
                new CustomerDto
                {
                    Id = UserId,
                    Email = "customer@example.com",
                    Latitude = 52,
                    Longitude = -2
                }
            ]);
        this.geometryCalculator
            .Setup(calculator => calculator.IsWithinRadius(52, -2, 51, -1, 20))
            .Returns(true);

        var result = await this.sut.GetUserIdsByLocationAndGenresAsync(
            51,
            -1,
            [Genre.Rock]);

        Assert.Equal([UserId], result);
    }

    [Fact]
    public async Task GetUserIdsByLocationAndGenresAsync_UserLookupFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.preferenceRepository
            .Setup(repository => repository.GetByMatchingGenresAsync(It.IsAny<IEnumerable<Genre>>()))
            .ReturnsAsync([NewPreference(UserId)]);
        this.userModule
            .Setup(module => module.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.GetUserIdsByLocationAndGenresAsync(
                51,
                -1,
                [Genre.Rock]));

        Assert.Same(expected, actual);
    }

    #endregion

    private static PreferenceEntity NewPreference(Guid userId) =>
        PreferenceEntity.Create(userId, 20, [Genre.Rock]);

    private static PreferenceRequest NewRequest() => new()
    {
        RadiusKm = 30,
        Genres = [Genre.Rock, Genre.Jazz]
    };
}
