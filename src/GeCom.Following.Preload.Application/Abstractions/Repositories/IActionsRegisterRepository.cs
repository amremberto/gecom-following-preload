using GeCom.Following.Preload.Domain.Preloads.ActionsRegisters;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for ActionsRegister entities.
/// </summary>
public interface IActionsRegisterRepository : IRepository<ActionsRegister>
{
    /// <summary>
    /// Gets actions registers by document ID.
    /// </summary>
    /// <param name="docId">Document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of actions registers.</returns>
    Task<IEnumerable<ActionsRegister>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets actions registers by user creation.
    /// </summary>
    /// <param name="usuarioCreacion">User creation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of actions registers.</returns>
    Task<IEnumerable<ActionsRegister>> GetByUsuarioCreacionAsync(string usuarioCreacion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets actions registers by action type.
    /// </summary>
    /// <param name="accion">Action type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of actions registers.</returns>
    Task<IEnumerable<ActionsRegister>> GetByAccionAsync(string accion, CancellationToken cancellationToken = default);
}
