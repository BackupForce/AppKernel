using Domain.Gaming;

namespace Application.Abstractions.Gaming;

public sealed record Lottery539RngResult(
    LotteryNumbers Numbers,
    string Algorithm,
    string DerivedInput);
