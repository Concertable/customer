using Concertable.Customer.User.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.User.Api.Controllers;

[ApiController]
[Route("internal/users")]
[Authorize("UserClaimsScope")]
internal sealed class UserClaimsController : ControllerBase
{
    private readonly IUserModule userModule;

    public UserClaimsController(IUserModule userModule)
    {
        this.userModule = userModule;
    }

    [HttpGet("{sub:guid}/claims")]
    public async Task<ActionResult<ClaimDto[]>> GetClaims(Guid sub)
    {
        var users = await userModule.GetByIdsAsync([sub]);
        if (users.Count == 0)
            return Ok(Array.Empty<ClaimDto>());

        return Ok(new[] { new ClaimDto("role", "Customer") });
    }

    public sealed record ClaimDto(string Type, string Value);
}
