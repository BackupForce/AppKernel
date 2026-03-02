using FluentValidation;

namespace Application.Gaming.Tickets.Admin.Winnings;

internal sealed class RedeemWinningCommandValidator : AbstractValidator<RedeemWinningCommand>
{
    public RedeemWinningCommandValidator()
    {
        RuleFor(command => command.WinningId).NotEmpty();
    }
}
