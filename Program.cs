using Catalog.Service.Entities;
using Common.Library.MongoDB;
using Common.Library.MassTransit;
using Common.Library.Settings;
using Common.Library.Identity;
using Catalog.Service;
using Common.Library.Configuration;
using Common.Library.HealthChecks;
using Common.Library.Logging;
using Common.Library.OpenTelemetry;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.ConfigureAzureKeyVault(builder.Environment);

const string AllowedOriginSetting = "AllowedOrigin";
// Add services to the container.

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
builder.Services.AddMongo()
    .AddMongoRepository<Item>("items")
    .AddMassTransitWithMessageBroker(builder.Configuration)
    .AddJwtBearerAuthentication();

builder.Services.AddSeqLogging(builder.Configuration)
    .AddTracing(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.Read, policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog.readaccess", "catalog.fullaccess");
    });
    options.AddPolicy(Policies.Write, policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog.writeaccess", "catalog.fullaccess");
    });
});


builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddMongoDbHealthCheck();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(_builder =>
    {
        _builder.WithOrigins(app.Configuration[AllowedOriginSetting])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

}

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapCustomHealthChecks();




app.Run();
