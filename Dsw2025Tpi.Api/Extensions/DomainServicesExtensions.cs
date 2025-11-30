using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Extensions
{
    public static class DomainServicesExtensions
    {
       public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Dsw2025TpiContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Dsw2025TpiEntities"));
                options.UseSeeding((c, t) =>
                {
                    ((Dsw2025TpiContext)c).Seedwork<Product>("Sources\\products.json");
                    //((Dsw2025TpiContext)c).Seedwork<Customer>("Sources\\customers.json");
                });
            });

            services.AddScoped<IRepository, EfRepository>();
            services.AddScoped<IProductsManagementService, ProductsManagementService>();
            services.AddScoped<IOrdersManagementService, OrdersManagementService>();
            services.AddScoped<ICustomersManagementService, CustomersManagementService>();
            services.AddScoped<IAuthenticateManagementService, AuthenticateManagementService>();

            return services;
        }
    }
}
