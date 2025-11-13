using GeCom.Following.Preload.Contracts.Preload.States;
using GeCom.Following.Preload.Domain.Preloads.States;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class StateMappings
{
    public static StateResponse ToResponse(State state)
    {
        StateResponse result = new(
            state.EstadoId,
            state.Descripcion,
            state.Codigo,
            state.FechaCreacion,
            state.FechaBaja
        );

        return result;
    }
}

