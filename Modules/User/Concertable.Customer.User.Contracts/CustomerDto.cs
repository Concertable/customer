using Concertable.Kernel.Identity;

namespace Concertable.Customer.User.Contracts;

public record CustomerDto : IUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public Role Role { get; } = Role.Customer;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? County { get; set; }
    public string? Town { get; set; }
    public string BaseUrl { get; set; } = "/";
    public bool IsEmailVerified { get; set; } = true;
}
