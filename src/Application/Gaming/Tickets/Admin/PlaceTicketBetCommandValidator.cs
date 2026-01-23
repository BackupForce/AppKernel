using FluentValidation;

namespace Application.Gaming.Tickets.Admin;

internal sealed class PlaceTicketBetCommandValidator : AbstractValidator<PlaceTicketBetCommand>
{
    public PlaceTicketBetCommandValidator()
    {
        RuleFor(command => command.TicketId)
            .NotEmpty()
            .WithMessage("ticketId 不可為空白。");

        RuleFor(command => command.Numbers)
            .NotEmpty()
            .WithMessage("numbers 不可為空白。");

        RuleFor(command => command.ClientReference)
            .MaximumLength(128);

        RuleFor(command => command.Note)
            .MaximumLength(512);

        RuleFor(command => command.IdempotencyKey)
            .MaximumLength(128);
    }
}
