using Concertable.Customer.Review.Application.Interfaces;
using Concertable.Customer.Review.Infrastructure.Services;
using Concertable.Kernel.Identity;
using Moq;
using Reunion.Errors;
using Reunion.Validation;

namespace Concertable.Customer.Review.UnitTests.Services;

public sealed class VenueReviewServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const int VenueId = 7;

    private readonly Mock<IReviewValidator> reviewValidator;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly VenueReviewService sut;

    public VenueReviewServiceTests()
    {
        this.reviewValidator = new Mock<IReviewValidator>();
        this.currentUser = new Mock<ICurrentUser>();
        this.currentUser.SetupGet(user => user.Id).Returns(UserId);
        this.currentUser.SetupGet(user => user.IsAuthenticated).Returns(true);
        this.sut = new VenueReviewService(
            new Mock<IVenueReviewRepository>().Object,
            this.reviewValidator.Object,
            this.currentUser.Object);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_UnauthenticatedUser_ReturnsFalseWithoutValidation()
    {
        this.currentUser.SetupGet(user => user.IsAuthenticated).Returns(false);

        var result = await this.sut.CanCurrentUserReviewAsync(VenueId);

        Assert.False(result);
        this.reviewValidator.Verify(
            validator => validator.ValidateVenueAsync(It.IsAny<Guid>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_ValidResult_ReturnsTrue()
    {
        this.reviewValidator
            .Setup(validator => validator.ValidateVenueAsync(UserId, VenueId))
            .ReturnsAsync(ValidationResult.Valid());

        var result = await this.sut.CanCurrentUserReviewAsync(VenueId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_InvalidResult_ReturnsFalse()
    {
        var errors = new ValidationErrors(
            [new("VenueId", "No reviewable ticket exists for this venue.")]);
        this.reviewValidator
            .Setup(validator => validator.ValidateVenueAsync(UserId, VenueId))
            .ReturnsAsync(ValidationResult.Invalid(errors));

        var result = await this.sut.CanCurrentUserReviewAsync(VenueId);

        Assert.False(result);
    }
}
