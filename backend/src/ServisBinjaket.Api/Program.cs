using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Infrastructure;
using ServisBinjaket.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Auth ──────────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSecret = builder.Configuration["JWT_SECRET"] ?? "";
        var keyBytes = jwtSecret.Length >= 32
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
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

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

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        services.GetRequiredService<ILogger<Program>>()
            .LogWarning("ADMIN_DEFAULT_EMAIL or ADMIN_DEFAULT_PASSWORD not set — skipping default admin creation");
        return;
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
}

public partial class Program { }
