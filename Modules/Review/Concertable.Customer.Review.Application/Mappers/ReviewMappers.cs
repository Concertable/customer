namespace Concertable.Customer.Review.Application.Mappers;

internal static class ReviewMappers
{
    public static ReviewDto ToDto(this ReviewEntity review, string email) => new()
    {
        Id = review.Id,
        Stars = review.Stars,
        Details = review.Details,
        Email = email
    };
}
