using FluentValidation;

namespace Application.Users.RemoveGroup;

internal sealed class RemoveGroupFromUserCommandValidator : AbstractValidator<RemoveGroupFromUserCommand>
{
    public RemoveGroupFromUserCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("使用者識別碼不可為空。");

        RuleFor(command => command.GroupId)
            .NotEmpty()
            .WithMessage("群組識別碼不可為空。");
    }
}
