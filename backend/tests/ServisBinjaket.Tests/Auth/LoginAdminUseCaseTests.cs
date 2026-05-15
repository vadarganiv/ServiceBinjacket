using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Auth.DTOs;
using ServisBinjaket.Application.Auth.UseCases;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Infrastructure.Auth;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Tests.Auth;

public class LoginAdminUseCaseTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly LoginAdminUseCase _useCase;
    private readonly IPasswordHasher _hasher;

    public LoginAdminUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _hasher = new BcryptPasswordHasher();

        var repo = new AdminUserRepository(_db);
        var jwt = new FakeJwtService();
        _useCase = new LoginAdminUseCase(repo, jwt, _hasher);

        SeedAdmin();
    }

    public void Dispose() => _db.Dispose();

    private void SeedAdmin()
    {
        _db.AdminUsers.Add(new AdminUser
        {
            Email = "admin@test.com",
            PasswordHash = _hasher.Hash("correct-password"),
            DisplayName = "Test Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        _db.AdminUsers.Add(new AdminUser
        {
            Email = "inactive@test.com",
            PasswordHash = _hasher.Hash("correct-password"),
            DisplayName = "Inactive Admin",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccessWithToken()
    {
        var result = await _useCase.ExecuteAsync(new LoginRequestDto { Email = "admin@test.com", Password = "correct-password" });

        result.Success.Should().BeTrue();
        result.Token.Should().Be("fake-token");
        result.Admin!.Email.Should().Be("admin@test.com");
    }

    [Fact]
    public async Task Login_ValidCredentials_UpdatesLastLoginAt()
    {
        await _useCase.ExecuteAsync(new LoginRequestDto { Email = "admin@test.com", Password = "correct-password" });

        var admin = await _db.AdminUsers.FirstAsync(u => u.Email == "admin@test.com");
        admin.LastLoginAt.Should().NotBeNull();
        admin.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsFailed()
    {
        var result = await _useCase.ExecuteAsync(new LoginRequestDto { Email = "admin@test.com", Password = "wrong-password" });

        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
    }

    [Fact]
    public async Task Login_NonExistentEmail_ReturnsFailed()
    {
        var result = await _useCase.ExecuteAsync(new LoginRequestDto { Email = "nobody@test.com", Password = "any-password" });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Login_InactiveAdmin_ReturnsFailed()
    {
        var result = await _useCase.ExecuteAsync(new LoginRequestDto { Email = "inactive@test.com", Password = "correct-password" });

        result.Success.Should().BeFalse();
    }

    private class FakeJwtService : IJwtService
    {
        public string GenerateToken(int adminId, string email) => "fake-token";
    }
}
