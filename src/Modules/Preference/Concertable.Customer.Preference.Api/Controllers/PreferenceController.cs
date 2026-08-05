using Concertable.Customer.Preference.Application.DTOs;
using Concertable.Customer.Preference.Application.Interfaces;
using Concertable.Customer.Preference.Application.Requests;
using Concertable.Shared.Api.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Preference.Api.Controllers;

[Authorize(Policy = "Customer")]
[ApiController]
[Route("api/[controller]")]
internal sealed class PreferenceController : ControllerBase
{
    private readonly IPreferenceService preferenceService;

    public PreferenceController(IPreferenceService preferenceService)
    {
        this.preferenceService = preferenceService;
    }

    [HttpPost]
    public async Task<ActionResult<PreferenceDto>> Create([FromBody] PreferenceRequest request)
    {
        var result = await preferenceService.CreateAsync(request);
        return result.ToActionResult(_ => Created());
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PreferenceDto>> Update(int id, [FromBody] PreferenceRequest request)
    {
        var result = await preferenceService.UpdateAsync(id, request);
        return result.ToOkActionResult();
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetByUser()
    {
        var preference = await preferenceService.GetByUserAsync();
        return preference.Match<IActionResult>(Ok, NoContent);
    }
}
