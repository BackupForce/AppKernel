using FluentValidation;

namespace Application.Users.RemoveRole;

internal sealed class RemoveUserRoleCommandValidator : AbstractValidator<RemoveUserRoleCommand>
{
    public RemoveUserRoleCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithMessage("使用者識別碼不可為空。");

        RuleFor(command => command.RoleId)
            .NotEmpty()
            .WithMessage("角色識別碼不可為空。");
    }
}
