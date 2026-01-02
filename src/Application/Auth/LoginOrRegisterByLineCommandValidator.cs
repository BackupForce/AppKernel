using FluentValidation;

namespace Application.Auth;

internal sealed class LoginOrRegisterByLineCommandValidator : AbstractValidator<LoginOrRegisterByLineCommand>
{
    public LoginOrRegisterByLineCommandValidator()
    {
        RuleFor(command => command.LineUserId)
            .NotEmpty()
            .WithMessage(LineLoginErrors.LineUserIdRequired.Description);

        RuleFor(command => command.LineUserName)
            .NotEmpty()
            .WithMessage(LineLoginErrors.LineUserNameRequired.Description);
    }
}
