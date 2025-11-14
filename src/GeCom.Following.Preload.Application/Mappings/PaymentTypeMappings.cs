using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class PaymentTypeMappings
{
    public static PaymentTypeResponse ToResponse(PaymentType paymentType)
    {
        PaymentTypeResponse result = new(
            paymentType.Id,
            paymentType.Descripcion
        );

        return result;
    }
}

