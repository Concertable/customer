using Concertable.Customer.User.Contracts;

namespace Concertable.Customer.User.Application.Interfaces;

internal interface IUserService
{
    Task<CustomerDto> SaveLocationAsync(double latitude, double longitude);
    Task<CustomerDto?> GetMeAsync();
}
