using Microsoft.AspNetCore.Authorization;

namespace Concertable.Customer.User.Api.Authorization;

public sealed class CustomerAttribute : AuthorizeAttribute
{
    public CustomerAttribute()
    {
        Policy = "Customer";
    }
}
