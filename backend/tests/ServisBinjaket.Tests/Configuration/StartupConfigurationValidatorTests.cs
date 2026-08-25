using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ServisBinjaket.Api.Configuration;

namespace ServisBinjaket.Tests.Configuration;

public class StartupConfigurationValidatorTests
{
    [Fact]
    public void ValidateProduction_WithPlaceholderJwtSecret_Throws()
    {
        var configuration = BuildConfiguration(
            StartupConfigurationValidator.PlaceholderJwtSecret,
            adminPassword: null);

        var action = () => StartupConfigurationValidator.ValidateProduction(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT_SECRET*");
    }

    [Fact]
    public void ValidateProduction_WithoutBootstrapCredentials_DoesNotThrow()
    {
        var configuration = BuildConfiguration(
            "a-test-jwt-secret-that-is-at-least-32-bytes-long",
            adminPassword: null);

        var action = () => StartupConfigurationValidator.ValidateProduction(configuration);

        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateAdminBootstrap_WithPlaceholderPassword_Throws()
    {
        var configuration = BuildConfiguration(
            "a-test-jwt-secret-that-is-at-least-32-bytes-long",
            StartupConfigurationValidator.PlaceholderAdminPassword);

        var action = () => StartupConfigurationValidator.ValidateAdminBootstrap(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*ADMIN_DEFAULT_PASSWORD*");
    }

    [Fact]
    public void ValidateAdminBootstrap_WithStrongCredentials_DoesNotThrow()
    {
        var configuration = BuildConfiguration(
            "a-test-jwt-secret-that-is-at-least-32-bytes-long",
            "a-strong-admin-password");

        var action = () => StartupConfigurationValidator.ValidateAdminBootstrap(configuration);

        action.Should().NotThrow();
    }

    private static IConfiguration BuildConfiguration(string jwtSecret, string? adminPassword)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_SECRET"] = jwtSecret,
                ["ADMIN_DEFAULT_EMAIL"] = "admin@example.test",
                ["ADMIN_DEFAULT_PASSWORD"] = adminPassword
            })
            .Build();
    }
}
