using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.SetWinningNumbers;

public sealed record SetDrawWinningNumbersCommand(
    Guid DrawId,
    string? WinningNumbersRaw,
    IReadOnlyCollection<int>? WinningNumbers,
    bool ForceRecalculate,
    string? SourceNote) : ICommand;
