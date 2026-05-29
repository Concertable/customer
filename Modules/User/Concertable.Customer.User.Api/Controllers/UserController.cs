using Concertable.Customer.User.Api.Authorization;
using Concertable.Customer.User.Application.Interfaces;
using Concertable.Customer.User.Application.Requests;
using Concertable.Customer.User.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.User.Api.Controllers;

[AuthorizeCustomer]
[ApiController]
[Route("api/[controller]")]
internal class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpPut("location")]
    public async Task<ActionResult<CustomerDto>> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var user = await userService.SaveLocationAsync(request.Latitude, request.Longitude);
        return Ok(user);
    }

    [HttpGet("me")]
    public async Task<ActionResult<CustomerDto>> Me()
    {
        var user = await userService.GetMeAsync();
        if (user is null) return Unauthorized();
        return Ok(user);
    }
}
