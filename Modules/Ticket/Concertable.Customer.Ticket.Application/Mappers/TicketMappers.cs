using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Domain.Entities;

namespace Concertable.Customer.Ticket.Application.Mappers;

internal static class TicketMappers
{
    public static TicketDto ToDto(this TicketEntity ticket, string email) => new()
    {
        Id = ticket.Id,
        PurchaseDate = ticket.PurchaseDate,
        QrCode = ticket.QrCode,
        UserId = ticket.UserId,
        UserEmail = email,
        Concert = new TicketConcert
        {
            Id = ticket.ConcertId,
            Name = ticket.ConcertName,
            Price = ticket.Price,
            StartDate = ticket.Period.Start,
            EndDate = ticket.Period.End,
            VenueName = ticket.VenueName,
            ArtistName = ticket.ArtistName
        }
    };

    public static IEnumerable<TicketDto> ToDtos(this IEnumerable<TicketEntity> tickets, string email) =>
        tickets.Select(t => t.ToDto(email));
}
