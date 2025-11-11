using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetAllDocuments;

/// <summary>
/// Query to get all documents.
/// </summary>
public sealed record GetAllDocumentsQuery : IQuery<IEnumerable<DocumentResponse>>;

