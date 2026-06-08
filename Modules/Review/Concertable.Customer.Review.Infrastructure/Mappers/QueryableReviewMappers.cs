using Concertable.Contracts;
using Concertable.Customer.Review.Domain.Entities;

namespace Concertable.Customer.Review.Infrastructure.Mappers;

internal static class QueryableReviewMappers
{
    public static IQueryable<ReviewDto> ToDto(this IQueryable<ReviewEntity> query) =>
        query.Select(r => new ReviewDto
        {
            Id = r.Id,
            Stars = r.Stars,
            Details = r.Details,
            Email = r.Email
        });
}
