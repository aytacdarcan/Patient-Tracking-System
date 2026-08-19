using HastaTakip.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CeddController : ControllerBase
{
    private readonly CeddService _ceddService;

    public CeddController(CeddService ceddService)
    {
        _ceddService = ceddService;
    }

    [HttpGet("calculate")]
    public async Task<IActionResult> Calculate(
    [FromQuery] string sex,
    [FromQuery] double age,
    [FromQuery] double height,
    [FromQuery] double weight)
    {
        var json = await _ceddService.CalculateAsync(sex, age, height, weight,null);

        return Content(json, "application/json");
    }
}