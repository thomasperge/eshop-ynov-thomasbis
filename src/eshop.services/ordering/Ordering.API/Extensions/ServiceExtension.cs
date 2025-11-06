using BuildingBlocks.Middlewares;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

namespace Ordering.API.Extensions;

/// <summary>
/// Provides extension methods for configuring and using API-specific services and middleware components
/// within the Ordering API application.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Configures and registers API-specific services, middleware components, and health checks for the application.
    /// </summary>
    /// <param name="services">The service collection to which the services will be added.</param>
    /// <param name="configuration">The application configuration used to retrieve settings and connection strings.</param>
    /// <returns>The updated service collection with API-specific services registered.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ordering API",
                Version = "v1",
                Description = "API pour la gestion des commandes",
                Contact = new OpenApiContact
                {
                    Name = "eShop Team",
                    Email = "support@eshop.com"
                }
            });
        });

        var connectionString = configuration.GetConnectionString("OrderingConnection");
        services.AddHealthChecks()
            .AddSqlServer(connectionString!);
        
        return services;
    }

    /// <summary>
    /// Configures the application to use API-specific middleware components, health checks, and controllers.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The configured web application instance with API-specific middleware and components applied.</returns>
    public static WebApplication UseApiServices(this WebApplication app)
    {
        // Configure Swagger UI in Development
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API v1");
                c.RoutePrefix = string.Empty; // Swagger UI à la racine
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        app.UseHealthChecks("/health", new HealthCheckOptions()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapControllers();
        
        return app;
    }
}