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

        /* Every principal carries an `owner` claim so Payment can key payout accounts uniformly;
           a customer owns as themselves, so their owner key is their own user id. */
        return Ok(new[]
        {
            new ClaimDto("role", "Customer"),
            new ClaimDto("owner", sub.ToString())
        });
    }

    public sealed record ClaimDto(string Type, string Value);
}
