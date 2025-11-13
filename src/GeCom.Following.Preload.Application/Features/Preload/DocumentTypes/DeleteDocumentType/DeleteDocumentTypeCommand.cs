using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.DeleteDocumentType;

/// <summary>
/// Command to delete a document type by its ID.
/// </summary>
public sealed record DeleteDocumentTypeCommand(int TipoDocId) : ICommand;

