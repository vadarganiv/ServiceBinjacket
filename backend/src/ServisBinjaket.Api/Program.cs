using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Infrastructure;
using ServisBinjaket.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(opt =>
{
    opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opt.KnownNetworks.Clear();
    opt.KnownProxies.Clear();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Auth ──────────────────────────────────────────────────────────────────────
const string PlaceholderJwtSecret = "change_me_long_random_secret_at_least_32_bytes";
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "";

if (builder.Environment.IsProduction())
{
    if (string.IsNullOrEmpty(jwtSecret) || jwtSecret == PlaceholderJwtSecret || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    {
        throw new InvalidOperationException(
            "JWT_SECRET is not configured for Production. Set it to a strong random value of at least 32 bytes " +
            "(e.g. `openssl rand -base64 48`) and restart. Placeholder value is rejected.");
    }
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keyBytes = Encoding.UTF8.GetByteCount(jwtSecret) >= 32
            ? Encoding.UTF8.GetBytes(jwtSecret)
            : new byte[32]; // invalid key — any real request will fail validation

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["sb_admin_token"];
                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                var adminIdClaim = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (adminIdClaim is null || !int.TryParse(adminIdClaim, out var adminId))
                {
                    ctx.Fail("Invalid token claims");
                    return;
                }

                var issuedAt = ctx.SecurityToken.ValidFrom;
                var repo = ctx.HttpContext.RequestServices.GetRequiredService<IAdminUserRepository>();
                var admin = await repo.GetByIdAsync(adminId);

                if (admin is null || !admin.IsActive ||
                    (admin.LastLogoutAt.HasValue && issuedAt <= admin.LastLogoutAt.Value))
                {
                    ctx.Fail("Token invalidated");
                }
            }
        };
    });

builder.Services.AddAuthorization();

// ── Rate limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", ctx =>
        RateLimitPartition.GetSlidingWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                SegmentsPerWindow = 10,
                QueueLimit = 0
            }));

    options.RejectionStatusCode = 429;
    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.Headers.RetryAfter = "600";
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { error = new { code = "RATE_LIMITED", message = "Too many login attempts. Try again later." } },
            token);
    };
});

// ── CORS ──────────────────────────────────────────────────────────────────────
var corsOrigins = (builder.Configuration["CORS_ORIGINS"] ?? "http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedAdminAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRateLimiter();

var uploadsRoot = app.Configuration["UPLOADS_ROOT"] ?? "/app/uploads";
if (!Directory.Exists(uploadsRoot))
    Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.GetFullPath(uploadsRoot)),
    RequestPath = "/uploads",
    ContentTypeProvider = new FileExtensionContentTypeProvider(),
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task SeedAdminAsync(IServiceProvider services)
{
    var db = services.GetRequiredService<AppDbContext>();
    if (await db.AdminUsers.AnyAsync())
        return;

    var email = services.GetRequiredService<IConfiguration>()["ADMIN_DEFAULT_EMAIL"];
    var password = services.GetRequiredService<IConfiguration>()["ADMIN_DEFAULT_PASSWORD"];
    var logger = services.GetRequiredService<ILogger<Program>>();

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        logger.LogWarning("ADMIN_DEFAULT_EMAIL or ADMIN_DEFAULT_PASSWORD not set — skipping default admin creation");
        return;
    }

    const string PlaceholderPassword = "change_me";
    if (password == PlaceholderPassword)
    {
        logger.LogWarning(
            "ADMIN_DEFAULT_PASSWORD is set to the placeholder value '{Placeholder}'. " +
            "Change it immediately after first login by updating the admin_users table directly. " +
            "See docs/DEPLOYMENT.md for the procedure.",
            PlaceholderPassword);
    }

    var hasher = services.GetRequiredService<IPasswordHasher>();
    db.AdminUsers.Add(new ServisBinjaket.Domain.Entities.AdminUser
    {
        Email = email,
        PasswordHash = hasher.Hash(password),
        DisplayName = "Admin",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    });
    await db.SaveChangesAsync();

    logger.LogInformation("Created default admin user with email {Email}", email);
}

public partial class Program { }
