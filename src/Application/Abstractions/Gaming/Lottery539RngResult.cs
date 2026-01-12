using Domain.Gaming;

namespace Application.Abstractions.Gaming;

/// <summary>
/// RNG 結果，包含中獎號碼與可驗證的 proof 資訊。
/// </summary>
public sealed record Lottery539RngResult(
    LotteryNumbers Numbers,
    string Algorithm,
    string DerivedInput);
