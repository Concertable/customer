using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Review.Api.Controllers;

[ApiController]
[Route("api/concerts/{concertId}/reviews")]
internal class ConcertReviewsController : ControllerBase
{
    private readonly IConcertReviewService reviewService;

    public ConcertReviewsController(IConcertReviewService reviewService)
    {
        this.reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(int concertId, [FromBody] CreateReviewRequest request)
    {
        request.ConcertId = concertId;
        var review = await reviewService.CreateAsync(request);

        return CreatedAtAction(nameof(GetByConcertId), new { concertId }, review);
    }

    [HttpGet]
    public async Task<ActionResult<IPagination<ReviewDto>>> GetByConcertId(int concertId, [FromQuery] PageParams pageParams) =>
        Ok(await reviewService.GetAsync(concertId, pageParams));

    [HttpGet("summary")]
    public async Task<ActionResult<ReviewSummaryDto>> GetSummary(int concertId) =>
        Ok(await reviewService.GetSummaryAsync(concertId));

    [HttpGet("eligibility")]
    public async Task<ActionResult<bool>> CanCurrentUserReview(int concertId) =>
        Ok(await reviewService.CanCurrentUserReviewAsync(concertId));
}
