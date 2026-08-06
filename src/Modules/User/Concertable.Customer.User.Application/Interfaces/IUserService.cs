using Concertable.Customer.User.Contracts;
using Concertable.Kernel.Functional;

namespace Concertable.Customer.User.Application.Interfaces;

internal interface IUserService
{
    Task<CustomerDto> SaveLocationAsync(double latitude, double longitude);
    Task<Option<CustomerDto>> GetMeAsync();
}
