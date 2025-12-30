namespace Web.Api.Endpoints.Members.Requests;

public sealed record AdjustMemberPointsRequest(
    long Delta,
    string Remark,
    string ReferenceType = "admin_adjust",
    string? ReferenceId = null,
    bool AllowNegative = false);
