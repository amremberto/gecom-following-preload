using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for UserSocietyAssignment entities.
/// </summary>
public interface IUserSocietyAssignmentRepository : IRepository<UserSocietyAssignment>
{
    /// <summary>
    /// Gets all society assignments for a user by email.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user society assignments.</returns>
    Task<IEnumerable<UserSocietyAssignment>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

