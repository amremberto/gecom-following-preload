namespace GeCom.Following.Preload.SharedKernel.Results;

/// <summary>
/// Representa una respuesta paginada con metadatos y elementos.
/// </summary>
/// <typeparam name="T">Tipo de los elementos.</typeparam>
public sealed record PagedResponse<T>
{
    /// <summary>
    /// Crea una nueva instancia de <see cref="PagedResponse{T}"/>.
    /// </summary>
    public PagedResponse(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        Items = items ?? throw new ArgumentNullException(nameof(items));
        TotalCount = totalCount < 0 ? 0 : totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    /// <summary>
    /// Elementos de la página actual.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Total de elementos en la colección sin paginar.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Número de página (1-based).
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Tamaño de página.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Total de páginas calculado.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Indica si existe una página anterior.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Indica si existe una página siguiente.
    /// </summary>
    public bool HasNext => Page < TotalPages;
}


