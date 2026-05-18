using FluentValidation;

namespace Application.Users.AssignRole;

internal sealed class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("使用者識別碼不可為空。");

        RuleFor(command => command.RoleId)
            .GreaterThan(0)
            .WithMessage("角色識別碼不可為空。");
    }
}
