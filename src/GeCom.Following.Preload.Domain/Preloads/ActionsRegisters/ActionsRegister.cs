using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.ActionsRegisters;

/// <summary>
/// Represents an actions register entity in the preload system.
/// </summary>
/// <remarks>
/// This entity tracks actions performed on documents, including action descriptions,
/// user information, and timestamps for audit purposes.
/// </remarks>
public partial class ActionsRegister : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique actions register identifier.
    /// </summary>
    public int RegId { get; set; }

    /// <summary>
    /// Gets or sets the action performed.
    /// </summary>
    public string Accion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the action.
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the document identifier associated with this action.
    /// </summary>
    public int DocId { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who created this action.
    /// </summary>
    public string NombreCreacion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user who created this action.
    /// </summary>
    public string UsuarioCreacion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the creation date of this action.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the document associated with this action.
    /// </summary>
    public virtual Document Document { get; set; } = null!;
}
