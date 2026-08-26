using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Ticket.Domain.Entities;

namespace Concertable.Customer.Seed.Infrastructure.Factories;

public static class ReviewFactory
{
    public static ReviewEntity CreateForTicket(TicketEntity ticket, byte stars, string? details, string email) =>
        ReviewEntity
            .Create(ticket.Id, stars, details, email, ticket.ArtistId, ticket.VenueId, ticket.ConcertId)
            .Match(
                success: review => review,
                failure: _ => throw new InvalidOperationException("Seed review must be valid."));
}
