using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.CreateUserSocietyAssignment;

/// <summary>
/// Handler for the CreateUserSocietyAssignmentCommand.
/// </summary>
internal sealed class CreateUserSocietyAssignmentCommandHandler : ICommandHandler<CreateUserSocietyAssignmentCommand, UserSocietyAssignmentResponse>
{
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserSocietyAssignmentCommandHandler"/> class.
    /// </summary>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateUserSocietyAssignmentCommandHandler(
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<UserSocietyAssignmentResponse>> Handle(CreateUserSocietyAssignmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Crear la nueva UserSocietyAssignment
        UserSocietyAssignment userSocietyAssignment = new()
        {
            Email = request.Email,
            CuitClient = request.CuitClient,
            SociedadFi = request.SociedadFi
        };

        // Agregar al repositorio
        UserSocietyAssignment addedAssignment = await _userSocietyAssignmentRepository.AddAsync(userSocietyAssignment, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        UserSocietyAssignmentResponse response = UserSocietyAssignmentMappings.ToResponse(addedAssignment);

        return Result.Success(response);
    }
}

