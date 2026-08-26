using Concertable.Customer.Preference.Application.DTOs;
using Concertable.Customer.Preference.Application.Interfaces;
using Concertable.Customer.Preference.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reunion.AspNetCore.Mvc;

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
        return result.ToCreatedOrProblem();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PreferenceDto>> Update(int id, [FromBody] PreferenceRequest request)
    {
        var result = await preferenceService.UpdateAsync(id, request);
        return result.ToOkOrProblem();
    }

    [HttpGet("user")]
    public async Task<ActionResult<PreferenceDto>> GetByUser()
    {
        var preference = await preferenceService.GetByUserAsync();
        return preference.ToOkOrNoContent();
    }
}
