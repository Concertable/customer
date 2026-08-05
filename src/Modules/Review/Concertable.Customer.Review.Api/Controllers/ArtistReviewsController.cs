using Concertable.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Review.Api.Controllers;

[ApiController]
[Route("api/artists/{artistId}/reviews")]
internal sealed class ArtistReviewsController : ControllerBase
{
    private readonly IArtistReviewService reviewService;

    public ArtistReviewsController(IArtistReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IPagination<ReviewDto>>> Get(int artistId, [FromQuery] PageParams pageParams) =>
        Ok(await reviewService.GetAsync(artistId, pageParams));

    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewSummary>> GetSummary(int artistId) =>
        Ok(await reviewService.GetSummaryAsync(artistId));

    [HttpGet("eligibility")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> CanCurrentUserReview(int artistId) =>
        Ok(await reviewService.CanCurrentUserReviewAsync(artistId));
}
