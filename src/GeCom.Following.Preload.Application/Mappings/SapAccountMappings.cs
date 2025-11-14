using GeCom.Following.Preload.Contracts.Spd_Sap.SapAccounts;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class SapAccountMappings
{
    public static SapAccountResponse ToResponse(SapAccount account)
    {
        SapAccountResponse result = new(
            account.Accountnumber,
            account.Name,
            account.Address1City,
            account.Address1Stateorprovince,
            account.Address1Postalcode,
            account.Address1Line1,
            account.Telephone1,
            account.Fax,
            account.Address1Country,
            account.NewCuit,
            account.NewBloqueado,
            account.NewRubro,
            account.NewIibb,
            account.Emailaddress1,
            account.Customertypecode,
            account.NewGproveedor,
            account.Cbu
        );

        return result;
    }
}

