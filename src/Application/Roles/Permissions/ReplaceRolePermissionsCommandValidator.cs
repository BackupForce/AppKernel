using FluentValidation;

namespace Application.Roles.Permissions;

internal sealed class ReplaceRolePermissionsCommandValidator : AbstractValidator<ReplaceRolePermissionsCommand>
{
    public ReplaceRolePermissionsCommandValidator()
    {
        RuleFor(command => command.RoleId)
            .GreaterThan(0)
            .WithMessage("角色編號必須大於 0。");

        RuleFor(command => command.PermissionCodes)
            .NotNull()
            .WithMessage("權限代碼集合不可為 null。");
    }
}
