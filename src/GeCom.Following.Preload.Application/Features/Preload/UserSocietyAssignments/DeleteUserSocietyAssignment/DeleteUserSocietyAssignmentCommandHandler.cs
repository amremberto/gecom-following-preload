using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.DeleteUserSocietyAssignment;

/// <summary>
/// Handler for the DeleteUserSocietyAssignmentCommand.
/// </summary>
internal sealed class DeleteUserSocietyAssignmentCommandHandler : ICommandHandler<DeleteUserSocietyAssignmentCommand>
{
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteUserSocietyAssignmentCommandHandler"/> class.
    /// </summary>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteUserSocietyAssignmentCommandHandler(
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteUserSocietyAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el UserSocietyAssignment existe
        UserSocietyAssignment? userSocietyAssignment = await _userSocietyAssignmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (userSocietyAssignment is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "UserSocietyAssignment.NotFound",
                    $"UserSocietyAssignment with ID '{request.Id}' was not found."));
        }

        // Eliminar el UserSocietyAssignment
        await _userSocietyAssignmentRepository.RemoveByIdAsync(request.Id, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

