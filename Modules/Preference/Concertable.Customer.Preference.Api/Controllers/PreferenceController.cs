using Concertable.Customer.Preference.Application.DTOs;
using Concertable.Customer.Preference.Application.Interfaces;
using Concertable.Customer.Preference.Application.Requests;
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
    public async Task<IActionResult> Create([FromBody] CreatePreferenceRequest request)
    {
        await preferenceService.CreateAsync(request);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PreferenceDto preferenceDto) =>
        Ok(await preferenceService.UpdateAsync(preferenceDto));

    [HttpGet("user")]
    public async Task<IActionResult> GetByUser() =>
        Ok(await preferenceService.GetByUserAsync());
}
