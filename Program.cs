using Catalog.Service.Entities;
using Common.Library.MongoDB;
using Common.Library.MassTransit;
using Common.Library.Settings;
using Common.Library.Identity;
using Catalog.Service;

var builder = WebApplication.CreateBuilder(args);
const string AllowedOriginSetting = "AllowedOrigin";
// Add services to the container.

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
builder.Services.AddMongo()
    .AddMongoRepository<Item>("items")
    .AddMassTransitWithRabbitMq()
    .AddJwtBearerAuthentication();
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



var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(_builder =>
{
    _builder.WithOrigins(app.Configuration[AllowedOriginSetting])
        .AllowAnyHeader()
        .AllowAnyMethod();
});



app.Run();
