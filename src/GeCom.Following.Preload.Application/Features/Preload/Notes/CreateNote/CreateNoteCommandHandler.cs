using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.Notes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.CreateNote;

/// <summary>
/// Handler for the CreateNoteCommand.
/// </summary>
internal sealed class CreateNoteCommandHandler : ICommandHandler<CreateNoteCommand, NoteResponse>
{
    private readonly INoteRepository _noteRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateNoteCommandHandler"/> class.
    /// </summary>
    /// <param name="noteRepository">The note repository.</param>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateNoteCommandHandler(
        INoteRepository noteRepository,
        IDocumentRepository documentRepository,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<NoteResponse>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar que el documento existe
        Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);
        if (document is null)
        {
            return Result.Failure<NoteResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        // Crear la nueva entidad Note
        Note note = new()
        {
            DocId = request.DocId,
            Descripcion = request.Descripcion,
            UsuarioCreacion = request.UsuarioCreacion,
            FechaCreacion = DateTime.UtcNow
        };

        // Agregar al repositorio
        Note addedNote = await _noteRepository.AddAsync(note, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        NoteResponse response = NoteMappings.ToResponse(addedNote);

        return Result.Success(response);
    }
}

