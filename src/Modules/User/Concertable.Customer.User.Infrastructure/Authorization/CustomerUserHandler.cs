using Concertable.Customer.User.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.User.Infrastructure.Authorization;

internal sealed class CustomerUserRequirement : IAuthorizationRequirement { }

internal sealed class CustomerUserHandler : AuthorizationHandler<CustomerUserRequirement>
{
    private readonly UserDbContext db;

    public CustomerUserHandler(UserDbContext db)
    {
        this.db = db;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomerUserRequirement requirement)
    {
        var subClaim = context.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(subClaim, out var sub)) return;

        if (await db.Users.AnyAsync(u => u.Id == sub))
            context.Succeed(requirement);
    }
}
