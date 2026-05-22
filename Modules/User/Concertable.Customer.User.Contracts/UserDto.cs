namespace Concertable.Customer.User.Contracts;

public record UserDto(
    Guid Id,
    string Email,
    double? Latitude,
    double? Longitude,
    string? County,
    string? Town);
