using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Domain.Preloads.Societies;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class SocietyMappings
{
    public static SocietyResponse ToResponse(Society society)
    {
        var result = new SocietyResponse(
            society.SocId,
            society.Codigo,
            society.Descripcion,
            society.Cuit,
            society.FechaCreacion,
            society.FechaBaja,
            society.EsPrecarga
        );

        return result;
    }
}
