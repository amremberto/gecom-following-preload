using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.PreloadDocument;

/// <summary>
/// Command to preload a document with a PDF file.
/// </summary>
public sealed record PreloadDocumentCommand(
    byte[] FileContent,
    string FileName,
    string ContentType) : ICommand<DocumentResponse>;

