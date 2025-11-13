using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetAllDocumentTypes;

/// <summary>
/// Query to get all document types.
/// </summary>
public sealed record GetAllDocumentTypesQuery() : IQuery<IEnumerable<DocumentTypeResponse>>;

