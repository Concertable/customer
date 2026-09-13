using Concertable.Customer.User.Domain.Events;
using Concertable.Kernel;
using NetTopologySuite.Geometries;

namespace Concertable.Customer.User.Domain.Entities;

public sealed class UserEntity : IGuidEntity, IEventRaiser
{
    protected UserEntity() { }

    private UserEntity(Guid id, string email)
    {
        Id = id;
        Email = email;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public Point? Location { get; private set; }
    public Address? Address { get; private set; }

    private readonly EventRaiser events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => events.DomainEvents;
    public void ClearDomainEvents() => events.Clear();

    public static UserEntity FromRegistration(Guid id, string email)
    {
        var user = new UserEntity(id, email);
        user.events.Raise(new UserRegisteredDomainEvent(id, email));
        return user;
    }

    public void UpdateLocation(Point location, Address address)
    {
        Location = location;
        Address = address;
    }
}
