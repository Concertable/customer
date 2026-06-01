namespace Concertable.Customer.Review.Application.Requests;

internal sealed record CreateReviewRequest
{
    public int ConcertId { get; set; }
    public byte Stars { get; set; }
    public string? Details { get; set; }
}
