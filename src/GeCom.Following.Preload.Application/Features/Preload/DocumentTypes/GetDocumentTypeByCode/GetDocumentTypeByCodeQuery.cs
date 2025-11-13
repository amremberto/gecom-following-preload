using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.GetDocumentTypeByCode;

/// <summary>
/// Query to get a document type by its code.
/// </summary>
public sealed record GetDocumentTypeByCodeQuery(string Codigo) : IQuery<DocumentTypeResponse>;

