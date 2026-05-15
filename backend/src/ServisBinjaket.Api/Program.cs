using Serilog;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddScoped<IHealthService, HealthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS handled by nginx in production; not needed here
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
