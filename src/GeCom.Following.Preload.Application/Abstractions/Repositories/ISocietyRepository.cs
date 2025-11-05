using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for Society entities.
/// </summary>
public interface ISocietyRepository : IRepository<Society>
{
    /// <summary>
    /// Gets a society by its code.
    /// </summary>
    /// <param name="codigo">Society code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The society or null.</returns>
    Task<Society?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a society by its CUIT.
    /// </summary>
    /// <param name="cuit">Society CUIT.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The society or null.</returns>
    Task<Society?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default);
}
