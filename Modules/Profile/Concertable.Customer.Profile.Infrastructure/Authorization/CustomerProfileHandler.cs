using Concertable.Customer.Profile.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Profile.Infrastructure.Authorization;

internal sealed class CustomerProfileRequirement : IAuthorizationRequirement { }

internal sealed class CustomerProfileHandler : AuthorizationHandler<CustomerProfileRequirement>
{
    private readonly ProfileDbContext db;

    public CustomerProfileHandler(ProfileDbContext db)
    {
        this.db = db;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomerProfileRequirement requirement)
    {
        var subClaim = context.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(subClaim, out var sub)) return;

        if (await db.CustomerProfiles.AnyAsync(p => p.Sub == sub))
            context.Succeed(requirement);
    }
}
