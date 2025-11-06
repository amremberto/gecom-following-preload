using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.UpdateSociety;

/// <summary>
/// Command to update an existing society.
/// </summary>
public sealed record UpdateSocietyCommand(
    int Id,
    string Codigo,
    string Descripcion,
    string Cuit,
    bool? EsPrecarga) : ICommand<SocietyResponse>;

