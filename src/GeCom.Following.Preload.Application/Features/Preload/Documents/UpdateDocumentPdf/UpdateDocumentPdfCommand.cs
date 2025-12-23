using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocumentPdf;

/// <summary>
/// Command to update the PDF file associated with a document.
/// </summary>
public sealed record UpdateDocumentPdfCommand(
    int DocId,
    byte[] FileContent,
    string FileName,
    string ContentType,
    string UserEmail) : ICommand<DocumentResponse>;

