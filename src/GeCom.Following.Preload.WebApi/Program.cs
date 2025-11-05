using GeCom.Following.Preload.Application;
using GeCom.Following.Preload.Infrastructure;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// Add application services.
builder.Services.AddPreloadApplication();

// Add infrastructure services.
builder.Services.AddPreloadInfrastructure();

builder.Services.AddControllers();

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
