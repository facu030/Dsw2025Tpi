using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers; // Asegúrate de que esta línea esté presente
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen( o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Desarollo de software TPI",
                    Version = "v1",
                });
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Ingrese el token",
                    Type = SecuritySchemeType.ApiKey
                });
                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        builder.Services.AddHealthChecks();
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8
            };

        })
             .AddEntityFrameworkStores<AuthenticateContext>()
             .AddDefaultTokenProviders();



        var jwtConfig = builder.Configuration.GetSection("Jwt"); //Levantamos la configuracion del json
        var KeyText = jwtConfig["Key"] ?? throw new ArgumentNullException("JWT Key");//levanto la key del json y si es nullo me da una excepción
        var Key = Encoding.UTF8.GetBytes(KeyText); //la convertimos a bite

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;//esquemas por defecto a utilizar al serviici de identificacion
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; //esquemas por defecto a utilizar al serviici de identificacion
        })
            .AddJwtBearer(options => //Ahora le decimos a Jwt como debe estar configurado para generar el token
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Key)
                };
            });
        
        builder.Services.AddSingleton<JwtTokenService>();


        builder.Services.AddDbContext<Dsw2025TpiContext>(option =>
        {
            option.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));

            // Configuración del seeding:
            // Este código se ejecuta cuando la aplicación se inicia.
            // Asegúrate de que los archivos JSON (Customers.json y Products.json)
            // estén en la carpeta 'Sources' de tu proyecto y que su propiedad
            // 'Copy to Output Directory' esté configurada como 'Copy if newer' o 'Copy always'.
            // También, verifica que la sintaxis de los JSON sea perfecta (sin comas extra, comentarios, etc.).
            option.UseSeeding((c, t) =>
            {
                var context = (Dsw2025TpiContext)c;
                // Seed para Clientes
                context.Seedwork<Customer>("Sources\\Customers.json");
                // Seed para Productos
                context.Seedwork<Product>("Sources\\Products.json");
            });
        });

        builder.Services.AddScoped<IRepository, EfRepository>();
        builder.Services.AddScoped<ProductsManagementService>();
        builder.Services.AddScoped<OrderManagementService>();
        
      
        builder.Services.AddDbContext<AuthenticateContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));
        });





        var app = builder.Build();

        // Configure the HTTP request pipeline.
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseMiddleware<Dsw2025Tpi.Api.Middleware.ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

       
        app.MapControllers();


        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}