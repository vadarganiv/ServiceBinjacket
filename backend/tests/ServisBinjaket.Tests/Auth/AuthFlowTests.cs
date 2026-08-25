using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServisBinjaket.Application.Auth.DTOs;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Infrastructure.Auth;
using ServisBinjaket.Infrastructure.Persistence;

namespace ServisBinjaket.Tests.Auth;

public sealed class AuthFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Email = "portfolio-admin@example.test";
    private const string Password = "correct-password";

    private readonly WebApplicationFactory<Program> _factory;

    public AuthFlowTests(WebApplicationFactory<Program> factory)
    {
        var databaseName = $"AuthFlow-{Guid.NewGuid():N}";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JWT_SECRET"] = "test-secret-key-that-is-long-enough-32-chars"
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        });

        using var scope = _factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        database.AdminUsers.Add(new AdminUser
        {
            Email = Email,
            PasswordHash = passwordHasher.Hash(Password),
            DisplayName = "Portfolio Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        database.SaveChanges();
    }

    [Fact]
    public async Task Login_logout_login_issues_a_new_valid_session()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });

        using var firstLogin = await LoginAsync(client);
        Assert.Equal(HttpStatusCode.OK, firstLogin.StatusCode);
        SetAuthenticationCookie(client, firstLogin);

        using var logout = await client.PostAsync("/api/v1/auth/logout", content: null);
        Assert.True(
            logout.StatusCode == HttpStatusCode.OK,
            $"Logout returned {(int)logout.StatusCode}: " +
            string.Join(" | ", logout.Headers.Select(header => $"{header.Key}={string.Join(',', header.Value)}")));

        client.DefaultRequestHeaders.Remove("Cookie");
        using var secondLogin = await LoginAsync(client);
        Assert.Equal(HttpStatusCode.OK, secondLogin.StatusCode);
        SetAuthenticationCookie(client, secondLogin);

        using var me = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client) =>
        client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequestDto { Email = Email, Password = Password });

    private static void SetAuthenticationCookie(HttpClient client, HttpResponseMessage loginResponse)
    {
        var cookie = loginResponse.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("sb_admin_token=", StringComparison.Ordinal))
            .Split(';', 2)[0];

        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", cookie);
    }
}
