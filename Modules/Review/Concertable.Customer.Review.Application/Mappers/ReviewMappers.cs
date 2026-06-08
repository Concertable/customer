using Concertable.Contracts;
using Concertable.Customer.Review.Domain.Entities;

namespace Concertable.Customer.Review.Application.Mappers;

internal static class ReviewMappers
{
    public static ReviewDto ToDto(this ReviewEntity review) => new()
    {
        Id = review.Id,
        Stars = review.Stars,
        Details = review.Details,
        Email = review.Email
    };
}
