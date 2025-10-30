using Discount.Grpc.Data;
using Discount.Grpc.Data.Extensions;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// ✅ Autoriser à la fois HTTP/1.1 et HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(6062, o => o.Protocols =
        Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2);
});

// Add services to the container.
builder.Services.AddGrpc();

// ✅ Ajouter le support des contrôleurs REST (pour Postman)
builder.Services.AddControllers();

// Ajouter la base de données SQLite
builder.Services.AddDbContext<DiscountContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DiscountConnection")));

// ✅ Ajouter Swagger (pour tester plus facilement)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Exécuter la migration automatique
app.UseCustomMigration();

// ✅ Activer Swagger en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// ✅ Ajouter le mapping REST + gRPC
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<DiscountServiceServer>();
    endpoints.MapControllers();
});

// Message par défaut
app.MapGet("/",
    () => "Communication with gRPC endpoints must be made through a gRPC client. " +
          "For REST API, try /swagger or /api/discount");

app.Run();