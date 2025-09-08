using FluentValidation.AspNetCore;
using GAC.WMS.Integrations.API.Extensions.DependencyInjection;
using GAC.WMS.Integrations.Application.Extensions.DependencyInjection;
using GAC.WMS.Integrations.Infrastructure.Extensions.DependencyInjection;
using GAC.WMS.Integrations.Infrastructure.Persistence.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gac-wms-integration-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

// Add application services
GAC.WMS.Integrations.Application.Extensions.DependencyInjection.ApplicationServiceExtensions.AddApplicationServices(builder.Services);
GAC.WMS.Integrations.Application.Extensions.DependencyInjection.ApplicationServiceExtensions.AddApplicationServices(builder.Services);

// Add infrastructure services
GAC.WMS.Integrations.Infrastructure.Extensions.DependencyInjection.InfrastructureServiceExtensions.AddInfrastructureServices(builder.Services, builder.Configuration);

// Add validators
GAC.WMS.Integrations.Infrastructure.Persistence.Data.ValidationExtensions.AddValidators(builder.Services);

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GAC WMS Integrations API",
        Version = "v1",
        Description = "API for integrating external ERP systems with GAC's Warehouse Management System"
    });
});

// Add services for dependency injection
// Register database migration service
builder.Services.AddHostedService<DatabaseMigrationService>();

// Register API services
builder.Services.AddApiServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GAC WMS Integrations API v1"));
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

// Migrations are now handled by DatabaseMigrationService

app.Run();
