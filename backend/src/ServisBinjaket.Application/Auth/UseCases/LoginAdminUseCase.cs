using ServisBinjaket.Application.Auth.DTOs;
using ServisBinjaket.Application.Interfaces;

namespace ServisBinjaket.Application.Auth.UseCases;

public class LoginAdminUseCase
{
    private readonly IAdminUserRepository _repo;
    private readonly IJwtService _jwt;
    private readonly IPasswordHasher _hasher;

    public LoginAdminUseCase(IAdminUserRepository repo, IJwtService jwt, IPasswordHasher hasher)
    {
        _repo = repo;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async Task<LoginResult> ExecuteAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var admin = await _repo.GetByEmailAsync(dto.Email, ct);

        if (admin is null || !admin.IsActive || !_hasher.Verify(dto.Password, admin.PasswordHash))
            return LoginResult.Failed;

        var now = DateTime.UtcNow;
        admin.LastLoginAt = now;
        await _repo.UpdateAsync(admin, ct);

        var token = _jwt.GenerateToken(admin.Id, admin.Email);
        return LoginResult.Ok(token, new AdminMeDto
        {
            Id = admin.Id,
            Email = admin.Email,
            DisplayName = admin.DisplayName,
            LastLoginAt = now
        });
    }
}

public class LoginResult
{
    public bool Success { get; private init; }
    public string? Token { get; private init; }
    public AdminMeDto? Admin { get; private init; }

    public static LoginResult Failed => new() { Success = false };

    public static LoginResult Ok(string token, AdminMeDto admin) =>
        new() { Success = true, Token = token, Admin = admin };
}
