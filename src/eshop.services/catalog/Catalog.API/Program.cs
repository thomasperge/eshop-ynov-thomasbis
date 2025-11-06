using BuildingBlocks.Behaviors;
using BuildingBlocks.Middlewares;
using Catalog.API.Data;
using FluentValidation;
using HealthChecks.UI.Client;
using Marten;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();

// Mediator Pattern - CQRS
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));

});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Management PostGreSQl as NOSQL
builder.Services.AddMarten(options =>
    {
        options.Connection(configuration.GetConnectionString("CatalogConnection") ?? string.Empty);
    })
    .UseLightweightSessions();

// Initiate Database
if(builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

// Health Check
builder.Services.AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("CatalogConnection")!);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Catalog API",
        Version = "v1",
        Description = "API pour la gestion du catalogue de produits",
        Contact = new OpenApiContact
        {
            Name = "eShop Team",
            Email = "support@eshop.com"
        }
    });
    
    // Inclure les commentaires XML si disponibles
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
        c.RoutePrefix = string.Empty; // Swagger UI à la racine
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Global Exception
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Health check Endpoint
app.UseHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();