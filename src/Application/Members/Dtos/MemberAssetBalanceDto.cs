namespace Application.Members.Dtos;

public sealed record MemberAssetBalanceDto(
    Guid MemberId,
    string AssetCode,
    decimal Balance,
    DateTime UpdatedAt);
