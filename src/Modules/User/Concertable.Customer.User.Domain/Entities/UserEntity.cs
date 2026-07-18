using Concertable.Kernel;
using NetTopologySuite.Geometries;

namespace Concertable.Customer.User.Domain.Entities;

public sealed class UserEntity : IGuidEntity
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

    public static UserEntity FromRegistration(Guid id, string email) => new(id, email);

    public void UpdateLocation(Point location, Address address)
    {
        Location = location;
        Address = address;
    }
}
