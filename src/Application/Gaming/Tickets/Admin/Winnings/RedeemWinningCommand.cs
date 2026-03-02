using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin.Winnings;

public sealed record RedeemWinningCommand(Guid WinningId) : ICommand<AdminWinningDetailDto>;
