using GeCom.Following.Preload.Application;
using GeCom.Following.Preload.Infrastructure;
using GeCom.Following.Preload.WebApi.Configurations;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

// Loads the configuration from the appsettings.json and configuration json files.
ConfigurationsLoader.AddConfigurationJsonFiles(builder.Configuration, builder.Environment);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(true);
}

// Add application services.
builder.Services.AddPreloadApplication();

// Add infrastructure services.
builder.Services.AddPreloadInfrastructure();

builder.Services.AddControllers();

#endregion

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
