using Microsoft.AspNetCore.Authorization;

namespace Concertable.Customer.User.Api.Authorization;

public sealed class AuthorizeCustomerAttribute : AuthorizeAttribute
{
    public AuthorizeCustomerAttribute()
    {
        Policy = "Customer";
        Roles = "Customer";
    }
}
