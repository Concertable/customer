namespace Concertable.Customer.Concert.Domain;

public class ConcertEntity : IIdEntity
{
    public int Id { get; private set; }
    public int TotalTickets { get; private set; }
    public int AvailableTickets { get; private set; }
    public decimal Price { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public DateTime? DatePosted { get; private set; }

    private ConcertEntity() { }

    public static ConcertEntity Create(int concertId, int totalTickets, decimal price, DateRange period, DateTime? datePosted) => new()
    {
        Id = concertId,
        TotalTickets = totalTickets,
        AvailableTickets = totalTickets,
        Price = price,
        Period = period,
        DatePosted = datePosted
    };

    public void Update(int totalTickets, decimal price, DateRange period, DateTime? datePosted)
    {
        var sold = TotalTickets - AvailableTickets;
        TotalTickets = totalTickets;
        AvailableTickets = Math.Max(0, totalTickets - sold);
        Price = price;
        Period = period;
        DatePosted = datePosted;
    }

    public void DecrementAvailability(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (AvailableTickets - quantity < 0)
            throw new DomainException($"Not enough tickets available. Only {AvailableTickets} left.");
        AvailableTickets -= quantity;
    }

    public void RestoreAvailability(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be positive.");
        if (AvailableTickets + quantity > TotalTickets)
            throw new DomainException($"Restore would exceed capacity ({TotalTickets}).");
        AvailableTickets += quantity;
    }
}
