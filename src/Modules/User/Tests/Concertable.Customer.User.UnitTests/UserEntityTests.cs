using Concertable.Customer.User.Domain;
using Concertable.Kernel;
using NetTopologySuite.Geometries;

namespace Concertable.Customer.User.UnitTests;

public sealed class UserEntityTests
{
    [Fact]
    public void FromRegistration_SetsIdAndEmail()
    {
        var id = Guid.NewGuid();

        var user = UserEntity.FromRegistration(id, "customer@test.com");

        Assert.Equal(id, user.Id);
        Assert.Equal("customer@test.com", user.Email);
        Assert.Null(user.Location);
        Assert.Null(user.Address);
    }

    [Fact]
    public void UpdateLocation_SetsLocationAndAddress()
    {
        var user = UserEntity.FromRegistration(Guid.NewGuid(), "customer@test.com");
        var location = new Point(-0.1276, 51.5072) { SRID = 4326 };
        var address = new Address("Greater London", "London");

        user.UpdateLocation(location, address);

        Assert.Equal(location, user.Location);
        Assert.Equal(address, user.Address);
    }
}
