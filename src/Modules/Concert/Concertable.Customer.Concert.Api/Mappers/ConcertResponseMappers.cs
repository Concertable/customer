using Concertable.Customer.Concert.Api.Responses;
using Concertable.Customer.Concert.Application.DTOs;

namespace Concertable.Customer.Concert.Api.Mappers;

internal static class ConcertResponseMappers
{
    public static ConcertDetailsResponse ToDetailsResponse(this ConcertDetails dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl,
        Avatar = dto.Avatar,
        Rating = dto.Rating,
        Price = dto.Price,
        TotalTickets = dto.TotalTickets,
        AvailableTickets = dto.AvailableTickets,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        DatePosted = dto.DatePosted,
        Venue = dto.Venue,
        Artist = dto.Artist,
        Genres = dto.Genres
    };
}
