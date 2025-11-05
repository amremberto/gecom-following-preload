using GeCom.Following.Preload.Domain.Preloads.ActionsRegisters;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.DocumentStates;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.Notes;
using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.Domain.Preloads.Providers;
using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Entities;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence;

/// <summary>
/// Represents the main database context for the application.
/// </summary>
public class PreloadDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PreloadDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public PreloadDbContext(DbContextOptions<PreloadDbContext> options)
        : base(options)
    {
    }

    // Preload entities
    public DbSet<ActionsRegister> ActionsRegisters => Set<ActionsRegister>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentState> DocumentStates => Set<DocumentState>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<PaymentDetail> PaymentDetails => Set<PaymentDetail>();
    public DbSet<PaymentType> PaymentTypes => Set<PaymentType>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<Society> Societies => Set<Society>();
    public DbSet<State> States => Set<State>();

    /// <summary>
    /// Configures the entity models and their relationships.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the entity models.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set the default collation for the database
        modelBuilder
            .UseCollation("Modern_Spanish_CI_AS");

        // Apply all configurations from the current assembly
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(PreloadDbContext).Assembly);

        // Configure global query filters if needed
        // Example: Soft delete filter
        // modelBuilder.Entity<SomeEntity>().HasQueryFilter(e => !e.IsDeleted)
    }

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Process domain events before saving
        await ProcessDomainEventsAsync();

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Processes domain events raised by entities.
    /// </summary>
    private Task ProcessDomainEventsAsync()
    {
        var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (BaseEntity entity in entitiesWithEvents)
        {
            // Here you would typically publish domain events through a mediator
            // For now, we'll just clear them
            entity.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }
}
