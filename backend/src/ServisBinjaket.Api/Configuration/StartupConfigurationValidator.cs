using System.Text;

namespace ServisBinjaket.Api.Configuration;

internal static class StartupConfigurationValidator
{
    internal const string PlaceholderJwtSecret = "change_me_long_random_secret_at_least_32_bytes";
    internal const string PlaceholderAdminPassword = "change_me";

    public static void ValidateProduction(IConfiguration configuration)
    {
        var jwtSecret = configuration["JWT_SECRET"];
        if (string.IsNullOrWhiteSpace(jwtSecret) ||
            jwtSecret.Equals(PlaceholderJwtSecret, StringComparison.Ordinal) ||
            Encoding.UTF8.GetByteCount(jwtSecret) < 32)
        {
            throw new InvalidOperationException(
                "JWT_SECRET must be a non-placeholder value of at least 32 bytes in Production.");
        }
    }

    public static void ValidateAdminBootstrap(IConfiguration configuration)
    {
        var adminEmail = configuration["ADMIN_DEFAULT_EMAIL"];
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            throw new InvalidOperationException("ADMIN_DEFAULT_EMAIL must be configured in Production.");
        }

        var adminPassword = configuration["ADMIN_DEFAULT_PASSWORD"];
        if (string.IsNullOrWhiteSpace(adminPassword) ||
            adminPassword.Equals(PlaceholderAdminPassword, StringComparison.OrdinalIgnoreCase) ||
            adminPassword.Length < 12)
        {
            throw new InvalidOperationException(
                "ADMIN_DEFAULT_PASSWORD must be a non-placeholder value of at least 12 characters in Production.");
        }
    }
}
