using Microsoft.AspNetCore.Mvc;
using ServisBinjaket.Application.Interfaces;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { status = _healthService.GetStatus() });
    }
}
