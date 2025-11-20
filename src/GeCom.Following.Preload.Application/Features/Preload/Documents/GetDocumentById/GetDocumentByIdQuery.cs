using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentById;

/// <summary>
/// Query to get a document by its ID.
/// </summary>
public sealed record GetDocumentByIdQuery(int DocId) : IQuery<DocumentResponse>;

