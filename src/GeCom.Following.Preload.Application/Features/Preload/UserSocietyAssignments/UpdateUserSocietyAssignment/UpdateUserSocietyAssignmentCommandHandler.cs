using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.UpdateUserSocietyAssignment;

/// <summary>
/// Handler for the UpdateUserSocietyAssignmentCommand.
/// </summary>
internal sealed class UpdateUserSocietyAssignmentCommandHandler : ICommandHandler<UpdateUserSocietyAssignmentCommand, UserSocietyAssignmentResponse>
{
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserSocietyAssignmentCommandHandler"/> class.
    /// </summary>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateUserSocietyAssignmentCommandHandler(
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<UserSocietyAssignmentResponse>> Handle(UpdateUserSocietyAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el UserSocietyAssignment existe
        UserSocietyAssignment? userSocietyAssignment = await _userSocietyAssignmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (userSocietyAssignment is null)
        {
            return Result.Failure<UserSocietyAssignmentResponse>(
                Error.NotFound(
                    "UserSocietyAssignment.NotFound",
                    $"UserSocietyAssignment with ID '{request.Id}' was not found."));
        }

        // Actualizar los campos
        userSocietyAssignment.Email = request.Email;
        userSocietyAssignment.CuitClient = request.CuitClient;
        userSocietyAssignment.SociedadFi = request.SociedadFi;

        // Actualizar en el repositorio
        UserSocietyAssignment updatedAssignment = await _userSocietyAssignmentRepository.UpdateAsync(userSocietyAssignment, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        UserSocietyAssignmentResponse response = UserSocietyAssignmentMappings.ToResponse(updatedAssignment);

        return Result.Success(response);
    }
}

