using Basket.API.Data.Repositories;
using Basket.API.Services;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using BuildingBlocks.Middlewares;
using Discount.Grpc;
using FluentValidation;
using HealthChecks.UI.Client;
using Marten;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));

});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddMarten(options =>
    {
        options.Connection(configuration.GetConnectionString("BasketConnection") ?? string.Empty);
    })
    .UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, BasketRepositoryCache>();

builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = configuration.GetConnectionString("RedisConnection") ?? string.Empty;
        options.InstanceName = "basket-api";
    }
   );

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(configuration.GetValue<string>("GrpcSettings:DiscountUrl") ?? string.Empty);   
}).ConfigurePrimaryHttpMessageHandler (() =>
{
    var handler = new HttpClientHandler();
    
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

// Register Catalog.API HTTP Client for stock validation
var catalogApiUrl = configuration["CatalogSettings:BaseUrl"] ?? "http://localhost:5050";
builder.Services.AddHttpClient<ICatalogService, CatalogService>(client =>
{
    client.BaseAddress = new Uri(catalogApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddMessageBroker(configuration);

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("BasketConnection")!)
    .AddRedis(configuration.GetConnectionString("RedisConnection")!);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Basket API",
        Version = "v1",
        Description = "API pour la gestion des paniers d'achat",
        Contact = new OpenApiContact
        {
            Name = "eShop Team",
            Email = "support@eshop.com"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API v1");
        c.RoutePrefix = string.Empty; // Swagger UI à la racine
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();