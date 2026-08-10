using Concertable.Customer.Review.Domain.Events;
using Concertable.Kernel;
using Reunion;
using Reunion.Errors;

namespace Concertable.Customer.Review.Domain.Entities;

public sealed class ReviewEntity : IIdEntity, IEventRaiser
{
    public int Id { get; private set; }
    public Guid TicketId { get; private set; }
    public int ConcertId { get; private set; }
    public int ArtistId { get; private set; }
    public int VenueId { get; private set; }
    public byte Stars { get; private set; }
    public string Email { get; private set; } = null!;
    public string? Details { get; private set; }

    private readonly EventRaiser events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => events.DomainEvents;
    public void ClearDomainEvents() => events.Clear();

    private ReviewEntity() { }

    public static Result<ReviewEntity, ValidationErrors> Create(
        Guid ticketId,
        byte stars,
        string? details,
        string email,
        int artistId,
        int venueId,
        int concertId)
    {
        if (stars is < 1 or > 5)
        {
            var errors = new ValidationErrors(
                [new("Stars", "Stars must be between 1 and 5.")]);

            return Result.Failure<ReviewEntity, ValidationErrors>(errors);
        }

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
        review.events.Raise(new ReviewCreatedDomainEvent(ticketId, artistId, venueId, concertId, stars, email, details));
        return Result.Success<ReviewEntity, ValidationErrors>(review);
    }
}
