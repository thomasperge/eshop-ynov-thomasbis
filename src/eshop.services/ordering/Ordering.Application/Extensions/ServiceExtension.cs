using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Ordering.Application.Services;

namespace Ordering.Application.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddFeatureManagement();
        services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly());
        
        // Register Catalog.API HTTP Client
        var catalogApiUrl = configuration["CatalogSettings:BaseUrl"] ?? "http://localhost:5050";
        services.AddHttpClient<ICatalogService, CatalogService>(client =>
        {
            client.BaseAddress = new Uri(catalogApiUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        return services;
    }
}