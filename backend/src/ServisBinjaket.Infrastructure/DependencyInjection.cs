using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Orders.UseCases;
using ServisBinjaket.Application.Products.UseCases;
using ServisBinjaket.Application.RepairRequests.UseCases;
using ServisBinjaket.Application.RepairRequests.Validators;
using ServisBinjaket.Application.Services.UseCases;
using ServisBinjaket.Infrastructure.Persistence;
using ServisBinjaket.Infrastructure.Repositories;
using ServisBinjaket.Infrastructure.Services;

namespace ServisBinjaket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IHealthService, HealthService>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<GetProductsUseCase>();
        services.AddScoped<GetProductBySlugUseCase>();
        services.AddScoped<GetProductCategoriesUseCase>();

        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<GetServiceCategoriesUseCase>();
        services.AddScoped<GetServicesUseCase>();
        services.AddScoped<GetServiceBySlugUseCase>();

        services.AddScoped<IRepairRequestRepository, RepairRequestRepository>();
        services.AddScoped<CreateRepairRequestUseCase>();

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<CreateOrderUseCase>();

        services.AddValidatorsFromAssemblyContaining<RepairRequestCreateValidator>();

        return services;
    }
}
