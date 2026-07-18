using Concertable.B2B.Seed.Contracts;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Preference.Domain;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Seed.Infrastructure.Factories;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.User.Domain;
using Concertable.Customer.User.Domain.Factories;
using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Contracts.Enums;
using Concertable.Seed.Identity;

namespace Concertable.Customer.Seed.Infrastructure;

public sealed class SeedState
{
    public const string TestPassword = "Password11!";

    // Customer users — typed entities, B2B style (decision A).
    public UserEntity Customer1 { get; }
    public UserEntity Customer2 { get; }
    public UserEntity Customer3 { get; }
    public IReadOnlyList<UserEntity> Customers { get; }

    // Customer-owned entities, built via Customer-side factories.
    public IReadOnlyList<PreferenceEntity> Preferences { get; }
    public IReadOnlyList<TicketEntity> Tickets { get; }
    public IReadOnlyList<ReviewEntity> Reviews { get; }

    // Named ticket / review handles.
    public TicketEntity UpcomingFlatFeeTicket { get; }
    public TicketEntity PastDoorSplitTicket { get; }
    public TicketEntity PastFlatFeeTicket { get; }
    public ReviewEntity ConfirmedConcertReview { get; }

    // Read-model entities built from the canonical SeedCatalog; XProjectionTestSeeders persist these.
    public VenueEntity Venue { get; }
    public IReadOnlyList<VenueEntity> Venues { get; }
    public ArtistEntity Artist { get; }
    public IReadOnlyList<ArtistEntity> Artists { get; }
    public ConcertEntity UpcomingFlatFeeConcert { get; }
    public ConcertEntity PastDoorSplitConcert { get; }
    public ConcertEntity PastFlatFeeConcert { get; }
    public IReadOnlyList<ConcertEntity> Concerts { get; }

    public SeedState(SeedCatalog catalog)
    {
        Customer1 = UserFactory.FromRegistration(SeedCustomers.CustomerId(1), SeedCustomers.CustomerEmail(1));
        Customer2 = UserFactory.FromRegistration(SeedCustomers.CustomerId(2), SeedCustomers.CustomerEmail(2));
        Customer3 = UserFactory.FromRegistration(SeedCustomers.CustomerId(3), SeedCustomers.CustomerEmail(3));
        Customers = [Customer1, Customer2, Customer3];

        Preferences =
        [
            PreferenceFactory.Create(Customer1.Id, 10, [Genre.Rock]),
            PreferenceFactory.Create(Customer2.Id, 25, []),
            PreferenceFactory.Create(Customer3.Id, 50, []),
        ];

        Venues = catalog.Venues.Select(s => s.ToEntity()).ToList();
        Venue = Venues[0];
        Artists = catalog.Artists.Select(s => s.ToEntity()).ToList();
        Artist = Artists[0];
        Concerts = catalog.Concerts.Select(s => s.ToEntity()).ToList();
        UpcomingFlatFeeConcert = Concerts.First(c => c.Name == "Upcoming FlatFee Show");
        PastDoorSplitConcert = Concerts.First(c => c.Name == "Past DoorSplit Show");
        PastFlatFeeConcert = Concerts.First(c => c.Name == "Past FlatFee Show");

        UpcomingFlatFeeTicket = UpcomingFlatFeeConcert.ToTicket(TicketId(1), Customer1.Id, catalog.Now);
        PastDoorSplitTicket = PastDoorSplitConcert.ToTicket(TicketId(2), Customer1.Id, catalog.Now);
        PastFlatFeeTicket = PastFlatFeeConcert.ToTicket(TicketId(3), Customer1.Id, catalog.Now);
        Tickets = [UpcomingFlatFeeTicket, PastDoorSplitTicket, PastFlatFeeTicket];

        // PastFlatFeeTicket is left unreviewed so tests have a review-eligible past ticket.
        ConfirmedConcertReview = ReviewFactory.CreateForTicket(PastDoorSplitTicket, 5, "Great show", Customer1.Email);
        PastDoorSplitTicket.MarkReviewed();
        Reviews = [ConfirmedConcertReview];
    }

    private static Guid TicketId(int n) => new($"d0000000-0000-0000-0000-{n:D12}");
}
