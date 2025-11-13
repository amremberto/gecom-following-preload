using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class DocumentTypeMappings
{
    public static DocumentTypeResponse ToResponse(DocumentType documentType)
    {
        DocumentTypeResponse result = new(
            documentType.TipoDocId,
            documentType.Descripcion,
            documentType.Letra,
            documentType.Codigo,
            documentType.DescripcionLarga,
            documentType.FechaCreacion,
            documentType.FechaBaja,
            documentType.IsFec
        );

        return result;
    }
}

