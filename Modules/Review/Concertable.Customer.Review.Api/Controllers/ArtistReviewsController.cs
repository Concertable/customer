using Concertable.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Review.Api.Controllers;

[ApiController]
[Route("api/artists/{artistId}/reviews")]
internal class ArtistReviewsController : ControllerBase
{
    private readonly IArtistReviewService reviewService;

    public ArtistReviewsController(IArtistReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<IPagination<ReviewDto>>> Get(int artistId, [FromQuery] PageParams pageParams) =>
        Ok(await reviewService.GetAsync(artistId, pageParams));

    [HttpGet("eligibility")]
    public async Task<ActionResult<bool>> CanCurrentUserReview(int artistId) =>
        Ok(await reviewService.CanCurrentUserReviewAsync(artistId));
}
