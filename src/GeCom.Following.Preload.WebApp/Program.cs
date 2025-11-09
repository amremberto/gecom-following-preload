using GeCom.Following.Preload.WebApp.Components;
using GeCom.Following.Preload.WebApp.Configurations;
using GeCom.Following.Preload.WebApp.Extensions.Auth;
using Microsoft.AspNetCore.Components.Authorization;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

#region Add services to the container.

// Loads the configuration from the appsettings.json and configuration json files.
ConfigurationsLoader.AddConfigurationJsonFiles(builder.Configuration, builder.Environment);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(true);
}

// Register configuration settings with IOptions
builder.Services.AddConfigurationSettings(builder.Configuration);

// Add configuration validators (immediately after loading configurations)
builder.Services.AddConfigurationValidators(builder.Environment);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication state provider for Blazor Server
// This automatically configures the AuthenticationStateProvider needed for AuthorizeView
builder.Services.AddCascadingAuthenticationState();

// Add controllers for API endpoints (if needed)
builder.Services.AddControllers();

// Add HttpContextAccessor for accessing HttpContext in Blazor Server components
builder.Services.AddHttpContextAccessor();

// Habilitar DetailedErrors en desarrollo para ver excepciones detalladas
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddServerSideBlazor().AddCircuitOptions(options => options.DetailedErrors = true);
}

// Add OpenID Connect authentication services
builder.Services.AddPreloadAuthentication(builder.Configuration);

// Add authorization services with custom policies
builder.Services.AddPreloadAuthorization();

#endregion

WebApplication? app = builder.Build();

// Validate configuration at startup
app.ValidateConfiguration();

#region Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

// Use authentication (must be before UseAuthorization)
app.UseAuthentication();

// Use authorization
app.UseAuthorization();

app.MapStaticAssets();

// Map API controllers
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

#endregion

await app.RunAsync();
