using System.ComponentModel.DataAnnotations.Schema;
using Concertable.Customer.Review.Domain.Events;
using Concertable.Kernel;

namespace Concertable.Customer.Review.Domain.Entities;

[Table("Reviews")]
public class ReviewEntity : IIdEntity, IEventRaiser
{
    public int Id { get; private set; }
    public Guid TicketId { get; private set; }
    public int ConcertId { get; private set; }
    public int ArtistId { get; private set; }
    public int VenueId { get; private set; }
    public byte Stars { get; private set; }
    public string Email { get; private set; } = null!;
    public string? Details { get; private set; }

    private readonly EventRaiser _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.DomainEvents;
    public void ClearDomainEvents() => _events.Clear();

    private ReviewEntity() { }

    public static ReviewEntity Create(
        Guid ticketId,
        byte stars,
        string? details,
        string email,
        int artistId,
        int venueId,
        int concertId)
    {
        ValidateStars(stars);
        var review = new ReviewEntity
        {
            TicketId = ticketId,
            Stars = stars,
            Details = details,
            Email = email,
            ArtistId = artistId,
            VenueId = venueId,
            ConcertId = concertId
        };
        review._events.Raise(new ReviewCreatedDomainEvent(ticketId, artistId, venueId, concertId, stars, email, details));
        return review;
    }

    private static void ValidateStars(byte stars)
    {
        if (stars is < 1 or > 5)
            throw new DomainException("Stars must be between 1 and 5.");
    }
}
