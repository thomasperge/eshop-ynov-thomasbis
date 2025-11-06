using Discount.Grpc.Data;
using Discount.Grpc.Data.Extensions;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel with separate ports for REST and gRPC
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5052, listenOptions => listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenLocalhost(5053, listenOptions => listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddGrpc();

builder.Services.AddDbContext<DiscountContext>(options => options.UseSqlite(configuration.GetConnectionString("DiscountConnection")));

// Add business logic service
builder.Services.AddScoped<IDiscountCalculationService, DiscountCalculationService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Discount gRPC API",
        Version = "v1",
        Description = "API pour la gestion des réductions et coupons. Service gRPC avec endpoints REST.",
        Contact = new OpenApiContact
        {
            Name = "eShop Team",
            Email = "support@eshop.com"
        }
    });
});

var app = builder.Build();

app.UseCustomMigration();

// Configure the HTTP request pipeline
// IMPORTANT: MapGrpcService doit être appelé APRÈS MapControllers pour éviter le conflit HTTP/2
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Discount gRPC API v1");
        c.RoutePrefix = string.Empty; // Swagger UI à la racine
    });
}

app.MapControllers();
app.MapGrpcService<DiscountServiceServer>();

app.MapGet("/",
    () =>
        "Discount.Grpc service is running. Use /openapi/v1.json for API documentation or gRPC client for service calls.");

app.Run();