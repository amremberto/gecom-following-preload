using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.ActionsRegisters;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class ActionsRegisterRepository : GenericRepository<ActionsRegister, PreloadDbContext>, IActionsRegisterRepository
{
    public ActionsRegisterRepository(PreloadDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActionsRegister>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(ar => ar.DocId == docId)
            .OrderByDescending(ar => ar.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActionsRegister>> GetByUsuarioCreacionAsync(string usuarioCreacion, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioCreacion);
        return await GetQueryable()
            .Where(ar => ar.UsuarioCreacion == usuarioCreacion)
            .OrderByDescending(ar => ar.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActionsRegister>> GetByAccionAsync(string accion, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accion);
        return await GetQueryable()
            .Where(ar => ar.Accion == accion)
            .OrderByDescending(ar => ar.FechaCreacion)
            .ToListAsync(cancellationToken);
    }
}
