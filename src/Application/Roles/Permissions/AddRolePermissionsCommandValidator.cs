using FluentValidation;

namespace Application.Roles.Permissions;

internal sealed class AddRolePermissionsCommandValidator : AbstractValidator<AddRolePermissionsCommand>
{
    public AddRolePermissionsCommandValidator()
    {
        RuleFor(command => command.RoleId)
            .GreaterThan(0)
            .WithMessage("角色編號必須大於 0。");

        RuleFor(command => command.PermissionCodes)
            .Must(HasValidCodes)
            .WithMessage("至少需要一個有效的權限代碼。");
    }

    private bool HasValidCodes(IReadOnlyCollection<string> codes)
    {
        if (codes is null)
        {
            return false;
        }

        foreach (string code in codes)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                return true;
            }
        }

        return false;
    }
}
