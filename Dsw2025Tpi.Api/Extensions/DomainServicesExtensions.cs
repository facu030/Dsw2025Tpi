using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public static class DomainServicesExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
       

        services.AddScoped<IRepository, EfRepository>();
        services.AddScoped<IProductsManagementService, ProductsManagementService>();
        services.AddScoped<IOrdersManagementService, OrdersManagementService>();
        services.AddScoped<ICustomersManagementService, CustomersManagementService>();
        services.AddScoped<IAuthenticateManagementService, AuthenticateManagementService>();

        return services;
    }
}
