using Concertable.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Reunion.AspNetCore.Mvc;

namespace Concertable.Customer.Review.Api.Controllers;

[ApiController]
[Route("api/concerts/{concertId}/reviews")]
internal sealed class ConcertReviewsController : ControllerBase
{
    private readonly IConcertReviewService reviewService;

    public ConcertReviewsController(IConcertReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    [EnableRateLimiting("review")]
    public async Task<ActionResult<ReviewDto>> Create(int concertId, [FromBody] CreateReviewRequest request)
    {
        var result = await reviewService.CreateAsync(concertId, request);
        return result.ToCreatedAtActionOrProblem(nameof(GetByConcertId), new { concertId });
    }

    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("public-read")]
    public async Task<ActionResult<IPagination<ReviewDto>>> GetByConcertId(int concertId, [FromQuery] PageParams pageParams) =>
        Ok(await reviewService.GetAsync(concertId, pageParams));

    [HttpGet("summary")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewSummary>> GetSummary(int concertId) =>
        Ok(await reviewService.GetSummaryAsync(concertId));

    [HttpGet("eligibility")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> CanCurrentUserReview(int concertId) =>
        Ok(await reviewService.CanCurrentUserReviewAsync(concertId));
}
