using FluentValidation;

namespace Application.Users.ResetPassword;

internal sealed class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("使用者識別碼不可為空。");

        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .WithMessage("新密碼不可為空。");
    }
}
