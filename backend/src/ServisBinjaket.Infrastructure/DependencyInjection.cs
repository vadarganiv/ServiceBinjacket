using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServisBinjaket.Application.Auth.UseCases;
using ServisBinjaket.Application.Interfaces;
using ServisBinjaket.Application.Orders.UseCases;
using ServisBinjaket.Application.Products.UseCases;
using ServisBinjaket.Application.Products.Validators;
using ServisBinjaket.Application.RepairRequests.UseCases;
using ServisBinjaket.Application.RepairRequests.Validators;
using ServisBinjaket.Application.Services.UseCases;
using ServisBinjaket.Application.Services.Validators;
using ServisBinjaket.Infrastructure.Auth;
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

        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<LoginAdminUseCase>();
        services.AddScoped<GetCurrentAdminUseCase>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<GetProductsUseCase>();
        services.AddScoped<GetProductBySlugUseCase>();
        services.AddScoped<GetProductCategoriesUseCase>();
        services.AddScoped<CreateProductUseCase>();
        services.AddScoped<UpdateProductUseCase>();

        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<GetServiceCategoriesUseCase>();
        services.AddScoped<GetServicesUseCase>();
        services.AddScoped<GetServiceBySlugUseCase>();
        services.AddScoped<CreateServiceUseCase>();
        services.AddScoped<UpdateServiceUseCase>();

        services.AddScoped<IRepairRequestRepository, RepairRequestRepository>();
        services.AddScoped<CreateRepairRequestUseCase>();

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<CreateOrderUseCase>();

        services.AddValidatorsFromAssemblyContaining<RepairRequestCreateValidator>();
        services.AddValidatorsFromAssemblyContaining<ProductCreateValidator>();
        services.AddValidatorsFromAssemblyContaining<ServiceCreateValidator>();

        return services;
    }
}
