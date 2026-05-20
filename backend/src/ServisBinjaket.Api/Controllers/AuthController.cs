using System.Security.Claims;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServisBinjaket.Application.Auth.DTOs;
using ServisBinjaket.Application.Auth.UseCases;
using ServisBinjaket.Application.Interfaces;

namespace ServisBinjaket.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private const string CookieName = "sb_admin_token";

    private readonly LoginAdminUseCase _login;
    private readonly GetCurrentAdminUseCase _getMe;
    private readonly IAdminUserRepository _adminRepo;
    private readonly IValidator<LoginRequestDto> _validator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        LoginAdminUseCase login,
        GetCurrentAdminUseCase getMe,
        IAdminUserRepository adminRepo,
        IValidator<LoginRequestDto> validator,
        ILogger<AuthController> logger)
    {
        _login = login;
        _getMe = getMe;
        _adminRepo = adminRepo;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = "Validation failed", details = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) } });

        var result = await _login.ExecuteAsync(dto, ct);

        if (!result.Success)
        {
            _logger.LogWarning("Login failed for email {Email} from IP {Ip}", dto.Email, HttpContext.Connection.RemoteIpAddress);
            return Unauthorized(new { error = new { code = "UNAUTHORIZED", message = "Invalid credentials" } });
        }

        Response.Cookies.Append(CookieName, result.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(8),
            Path = "/"
        });

        return Ok(result.Admin);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var admin = await _adminRepo.GetByIdAsync(adminId, ct);
        if (admin is not null)
        {
            admin.LastLogoutAt = DateTime.UtcNow;
            await _adminRepo.UpdateAsync(admin, ct);
        }

        Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/" });
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var me = await _getMe.ExecuteAsync(adminId, ct);
        if (me is null)
            return Unauthorized(new { error = new { code = "UNAUTHORIZED", message = "Unauthorized" } });

        return Ok(me);
    }
}
