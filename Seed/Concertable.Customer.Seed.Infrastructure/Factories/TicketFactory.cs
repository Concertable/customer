using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Kernel;

namespace Concertable.Customer.Seed.Infrastructure.Factories;

public static class TicketFactory
{
    public static TicketEntity Create(
        Guid id,
        Guid userId,
        int concertId,
        DateTime purchaseDate,
        string concertName,
        decimal price,
        DateRange period,
        int artistId,
        string artistName,
        int venueId,
        string venueName) =>
        TicketEntity.Create(id, userId, concertId, [], purchaseDate, concertName, price, period, artistId, artistName, venueId, venueName);
}
