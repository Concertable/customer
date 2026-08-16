using Concertable.Customer.User.Contracts;
using Reunion;

namespace Concertable.Customer.User.Application.Interfaces;

internal interface IUserService
{
    Task<CustomerDto> SaveLocationAsync(double latitude, double longitude);
    Task<Option<CustomerDto>> GetMeAsync();
    Task<IReadOnlyList<CustomerDto>> GetByIdsAsync(IEnumerable<Guid> ids);
}
