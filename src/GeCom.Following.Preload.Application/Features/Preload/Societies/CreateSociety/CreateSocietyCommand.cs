using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Contracts.Preload.Societies.Create;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.CreateSociety;

/// <summary>
/// Command to create a new society.
/// </summary>
public sealed record CreateSocietyCommand(
    string Codigo,
    string Descripcion,
    string Cuit,
    bool? EsPrecarga) : ICommand<SocietyResponse>;

