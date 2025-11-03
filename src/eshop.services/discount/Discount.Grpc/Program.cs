using Discount.Grpc.Data;
using Discount.Grpc.Data.Extensions;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

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

// Add OpenAPI/Swagger support
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCustomMigration();

// Configure the HTTP request pipeline
// IMPORTANT: MapGrpcService doit être appelé APRÈS MapControllers pour éviter le conflit HTTP/2
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapGrpcService<DiscountServiceServer>();

app.MapGet("/",
    () =>
        "Discount.Grpc service is running. Use /openapi/v1.json for API documentation or gRPC client for service calls.");

app.Run();