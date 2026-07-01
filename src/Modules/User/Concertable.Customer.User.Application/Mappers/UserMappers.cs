using Concertable.Customer.User.Contracts;
using Concertable.Customer.User.Domain;

namespace Concertable.Customer.User.Application.Mappers;

internal static class UserMappers
{
    public static CustomerDto ToDto(this UserEntity user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Latitude = user.Location?.Y,
        Longitude = user.Location?.X,
        County = user.Address?.County,
        Town = user.Address?.Town
    };
}
