namespace Concertable.Customer.Review.Application.Requests;

internal sealed record CreateReviewRequest
{
    public byte Stars { get; init; }
    public string? Details { get; init; }
}
