using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.CreateDocumentType;

/// <summary>
/// Command to create a new document type.
/// </summary>
public sealed record CreateDocumentTypeCommand(
    string Descripcion,
    string? Letra,
    string Codigo,
    string? DescripcionLarga,
    bool IsFec) : ICommand<DocumentTypeResponse>;

