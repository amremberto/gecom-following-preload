namespace GeCom.Following.Preload.Application.Abstractions.Monitor;

/// <summary>
/// Request to look up a document in [Monitores].[dbo].[Documents] and get its SapDocumentNumber.
/// </summary>
public sealed record GetSapDocumentNumberRequest(
    string DocumentNumber,
    string ProviderNumber,
    string ClientNumber,
    string SalePoint,
    string Letter);
