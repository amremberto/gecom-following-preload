using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Attachments;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class AttachmentMappings
{
    public static AttachmentResponse ToResponse(Attachment attachment)
    {
        AttachmentResponse result = new(
            attachment.AdjuntoId,
            attachment.Path,
            attachment.FechaCreacion,
            attachment.FechaModificacion,
            attachment.FechaBorrado,
            attachment.DocId
        );

        return result;
    }
}

