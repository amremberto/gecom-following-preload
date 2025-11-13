using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.UpdateDocumentType;

/// <summary>
/// Command to update an existing document type.
/// </summary>
public sealed record UpdateDocumentTypeCommand(
    int TipoDocId,
    string Descripcion,
    string? Letra,
    string Codigo,
    string? DescripcionLarga,
    bool IsFec) : ICommand<DocumentTypeResponse>;

