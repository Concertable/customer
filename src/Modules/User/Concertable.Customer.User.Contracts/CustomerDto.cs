using Concertable.Kernel.Identity;

namespace Concertable.Customer.User.Contracts;

public sealed record CustomerDto : IUser
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public Role Role { get; } = Role.Customer;
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? County { get; init; }
    public string? Town { get; init; }
    public string BaseUrl { get; init; } = "/";
    public bool IsEmailVerified { get; init; } = true;
}
