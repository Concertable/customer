using Concertable.Customer.Review.Application.Interfaces;
using Concertable.Customer.Review.Infrastructure.Services;
using Concertable.Kernel.Identity;
using Moq;
using Reunion.Errors;
using Reunion.Validation;

namespace Concertable.Customer.Review.UnitTests.Services;

public sealed class ArtistReviewServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const int ArtistId = 5;

    private readonly Mock<IReviewValidator> reviewValidator;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly ArtistReviewService sut;

    public ArtistReviewServiceTests()
    {
        this.reviewValidator = new Mock<IReviewValidator>();
        this.currentUser = new Mock<ICurrentUser>();
        this.currentUser.SetupGet(user => user.Id).Returns(UserId);
        this.currentUser.SetupGet(user => user.IsAuthenticated).Returns(true);
        this.sut = new ArtistReviewService(
            new Mock<IArtistReviewRepository>().Object,
            this.reviewValidator.Object,
            this.currentUser.Object);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_UnauthenticatedUser_ReturnsFalseWithoutValidation()
    {
        this.currentUser.SetupGet(user => user.IsAuthenticated).Returns(false);

        var result = await this.sut.CanCurrentUserReviewAsync(ArtistId);

        Assert.False(result);
        this.reviewValidator.Verify(
            validator => validator.ValidateArtistAsync(It.IsAny<Guid>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_ValidResult_ReturnsTrue()
    {
        this.reviewValidator
            .Setup(validator => validator.ValidateArtistAsync(UserId, ArtistId))
            .ReturnsAsync(ValidationResult.Valid());

        var result = await this.sut.CanCurrentUserReviewAsync(ArtistId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_InvalidResult_ReturnsFalse()
    {
        var errors = new ValidationErrors(
            [new("ArtistId", "No reviewable ticket exists for this artist.")]);
        this.reviewValidator
            .Setup(validator => validator.ValidateArtistAsync(UserId, ArtistId))
            .ReturnsAsync(ValidationResult.Invalid(errors));

        var result = await this.sut.CanCurrentUserReviewAsync(ArtistId);

        Assert.False(result);
    }
}
