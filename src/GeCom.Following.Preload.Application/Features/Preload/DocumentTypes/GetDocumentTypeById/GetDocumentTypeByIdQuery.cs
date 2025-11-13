using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetDocumentTypeById;

/// <summary>
/// Query to get a document type by its ID.
/// </summary>
public sealed record GetDocumentTypeByIdQuery(int TipoDocId) : IQuery<DocumentTypeResponse>;

