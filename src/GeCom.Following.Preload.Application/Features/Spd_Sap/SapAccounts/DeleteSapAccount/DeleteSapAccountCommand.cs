using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapAccounts.DeleteSapAccount;

/// <summary>
/// Command to delete a SAP account by its account number.
/// </summary>
public sealed record DeleteSapAccountCommand(string Accountnumber) : ICommand;

