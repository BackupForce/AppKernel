using FluentValidation;

namespace Application.Gaming.Tickets.Admin;

internal sealed class IssueMemberTicketsCommandValidator : AbstractValidator<IssueMemberTicketsCommand>
{
    public IssueMemberTicketsCommandValidator()
    {
        RuleFor(command => command.MemberId)
            .NotEmpty()
            .WithMessage("memberId 不可為空白。");

        RuleFor(command => command.GameCode)
            .NotEmpty()
            .WithMessage("gameCode 不可為空白。");

        RuleFor(command => command.PlayTypeCode)
            .NotEmpty()
            .WithMessage("playTypeCode 不可為空白。");

        RuleFor(command => command.DrawId)
            .NotEmpty()
            .WithMessage("drawId 不可為空白。");

        RuleFor(command => command.Quantity)
            .InclusiveBetween(1, 100)
            .WithMessage("quantity 必須介於 1 到 100。");

        RuleFor(command => command.Reason)
            .MaximumLength(256);

        RuleFor(command => command.Note)
            .MaximumLength(512);

        RuleFor(command => command.IdempotencyKey)
            .MaximumLength(128);
    }
}
