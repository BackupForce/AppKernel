namespace Web.Api.Endpoints.Members.Requests;

public sealed record AdjustMemberAssetRequest(
    string AssetCode,
    decimal Delta,
    string Remark,
    string ReferenceType,
    string? ReferenceId = null,
    bool AllowNegative = false);
