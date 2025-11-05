using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence;

/// <summary>
/// Represents the database context for SAP-related entities.
/// </summary>
/// <remarks>
/// This context manages SAP entities including accounts and purchase orders
/// from the SAP system integration.
/// </remarks>
public class SpdSapDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpdSapDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public SpdSapDbContext(DbContextOptions<SpdSapDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the SAP accounts entity set.
    /// </summary>
    public DbSet<SapAccount> SapAccounts => Set<SapAccount>();

    /// <summary>
    /// Gets the SAP purchase orders entity set.
    /// </summary>
    public DbSet<SapPurchaseOrder> SapPurchaseOrders => Set<SapPurchaseOrder>();

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

        // Add your custom configurations here
        modelBuilder
            .ApplyConfigurationsFromAssembly(typeof(SpdSapDbContext).Assembly);
    }
}
