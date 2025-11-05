using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for State entities.
/// </summary>
public interface IStateRepository : IRepository<State>
{
    /// <summary>
    /// Gets a state by its code.
    /// </summary>
    /// <param name="code">State code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The state or null.</returns>
    Task<State?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a state by its EstadoId.
    /// </summary>
    /// <param name="estadoId">State ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The state or null.</returns>
    Task<State?> GetByEstadoIdAsync(int estadoId, CancellationToken cancellationToken = default);
}
