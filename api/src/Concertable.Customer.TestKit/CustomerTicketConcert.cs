namespace Concertable.Customer.TestKit;

public sealed record CustomerTicketConcert
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string VenueName { get; init; } = null!;
    public string ArtistName { get; init; } = null!;
}
