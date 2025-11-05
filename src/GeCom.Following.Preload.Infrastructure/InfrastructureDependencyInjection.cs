using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Infrastructure.Persistence;
using GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeCom.Following.Preload.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddPreloadInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Add Preload DbContext
        services.AddDbContext<PreloadDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("PreloadConnection")
                ?? throw new InvalidOperationException("Connection string 'PreloadConnection' not found.");

            options.UseSqlServer(connectionString, sqlOptions
                => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
        });

        // Add SpdSap DbContext
        services.AddDbContext<SpdSapDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("SpdSapConnection")
                ?? throw new InvalidOperationException("Connection string 'SpdSapConnection' not found.");
            options.UseSqlServer(connectionString, sqlOptions
                => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
        });

        // Register specific repositories
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IPaymentTypeRepository, PaymentTypeRepository>();
        services.AddScoped<ISocietyRepository, SocietyRepository>();
        services.AddScoped<IActionsRegisterRepository, ActionsRegisterRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
