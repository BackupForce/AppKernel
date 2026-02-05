using System.Text.Json;

namespace Web.Api.Endpoints.Admin.Requests;

public sealed record SetDrawWinningNumbersRequest(
    JsonElement WinningNumbers,
    bool ForceRecalculate = false,
    string? SourceNote = null);
