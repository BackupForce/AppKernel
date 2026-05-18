using Application.Abstractions.Messaging;

namespace Application.Members.Assets.Adjust;

public sealed record AdjustMemberAssetCommand(
    Guid MemberId,
    string AssetCode,
    decimal Delta,
    string Remark,
    string ReferenceType,
    string? ReferenceId = null,
    bool AllowNegative = false) : ICommand<decimal>;
