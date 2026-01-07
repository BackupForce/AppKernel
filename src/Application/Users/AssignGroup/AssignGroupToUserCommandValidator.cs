using FluentValidation;

namespace Application.Users.AssignGroup;

internal sealed class AssignGroupToUserCommandValidator : AbstractValidator<AssignGroupToUserCommand>
{
    public AssignGroupToUserCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("使用者識別碼不可為空。");

        RuleFor(command => command.GroupId)
            .NotEmpty()
            .WithMessage("群組識別碼不可為空。");
    }
}
