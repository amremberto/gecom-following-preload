using GeCom.Following.Preload.Contracts.Preload.PaymentDetails;
using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class PaymentDetailMappings
{
    public static PaymentDetailResponse ToResponse(PaymentDetail paymentDetail)
    {
        PaymentDetailResponse result = new(
            paymentDetail.Id,
            paymentDetail.IdTipoDePago,
            paymentDetail.NroCheque,
            paymentDetail.Banco,
            paymentDetail.Vencimiento,
            paymentDetail.ImporteRecibido,
            paymentDetail.FechaAlta,
            paymentDetail.NamePdf
        );

        return result;
    }
}
