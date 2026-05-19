namespace Concertable.Customer.Ticket.Infrastructure.Services.Workflow;

internal sealed class ArtistTicketPayee : ITicketPayee
{
    public Guid Resolve(ConcertEntity concert, IContract contract) =>
        concert.Booking.Application.Artist.UserId;
}
