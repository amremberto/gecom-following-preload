using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.DeleteDocument;

/// <summary>
/// Command to logically delete a document by setting FechaBaja.
/// </summary>
public sealed record DeleteDocumentCommand(int DocId) : ICommand<DocumentResponse>;
