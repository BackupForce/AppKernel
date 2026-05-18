using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Roles.Permissions;
using Domain.Security;
using Domain.Users;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Roles;

public class ReplaceRolePermissionsCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly IAuthzCacheInvalidator _invalidator = Substitute.For<IAuthzCacheInvalidator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ReplaceRolePermissionsCommandHandler _handler;

    public ReplaceRolePermissionsCommandHandlerTests()
    {
        _userContext.UserType.Returns(UserType.Tenant);
        _userContext.TenantId.Returns(TenantId);
        _handler = new ReplaceRolePermissionsCommandHandler(_roleRepository, _userContext, _invalidator, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_Replace_A_B_With_B_C()
    {
        Role role = CreateTenantRoleWithPermissions(Permission.Members.View.Name, Permission.MemberTag.Create.Name);
        _roleRepository.GetByIdAsync(role.Id, true, Arg.Any<CancellationToken>()).Returns(role);

        Result result = await _handler.Handle(
            new ReplaceRolePermissionsCommand(role.Id, [Permission.MemberTag.Create.Name, Permission.MemberAudit.Read.Name]),
            default);

        result.IsSuccess.Should().BeTrue();
        await _roleRepository.Received(1).RemovePermissionsAsync(
            role.Id,
            Arg.Is<IEnumerable<string>>(codes => codes.OrderBy(x => x).SequenceEqual([Permission.Members.View.Name])),
            Arg.Any<CancellationToken>());
        await _roleRepository.Received(1).AddPermissionsAsync(
            Arg.Is<IEnumerable<Permission>>(permissions => permissions.Select(x => x.Name).OrderBy(x => x)
                .SequenceEqual([Permission.MemberAudit.Read.Name])),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _invalidator.Received(1).InvalidateRoleAsync(role.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Clear_All_Permissions_When_Request_Is_Empty()
    {
        Role role = CreateTenantRoleWithPermissions(Permission.Members.View.Name, Permission.MemberTag.Create.Name);
        _roleRepository.GetByIdAsync(role.Id, true, Arg.Any<CancellationToken>()).Returns(role);

        Result result = await _handler.Handle(new ReplaceRolePermissionsCommand(role.Id, Array.Empty<string>()), default);

        result.IsSuccess.Should().BeTrue();
        await _roleRepository.Received(1).RemovePermissionsAsync(
            role.Id,
            Arg.Is<IEnumerable<string>>(codes => codes.OrderBy(x => x).SequenceEqual([
                Permission.MemberTag.Create.Name,
                Permission.Members.View.Name
            ].OrderBy(x => x))),
            Arg.Any<CancellationToken>());
        await _roleRepository.DidNotReceive().AddPermissionsAsync(Arg.Any<IEnumerable<Permission>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Deduplicate_Duplicate_Codes_And_Be_Idempotent()
    {
        Role role = CreateTenantRoleWithPermissions(Permission.Members.View.Name);
        _roleRepository.GetByIdAsync(role.Id, true, Arg.Any<CancellationToken>()).Returns(role);

        Result result = await _handler.Handle(
            new ReplaceRolePermissionsCommand(role.Id, [Permission.Members.View.Name, Permission.Members.View.Name, " members:read "]),
            default);

        result.IsSuccess.Should().BeTrue();
        await _roleRepository.DidNotReceive().RemovePermissionsAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        await _roleRepository.DidNotReceive().AddPermissionsAsync(Arg.Any<IEnumerable<Permission>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _invalidator.DidNotReceive().InvalidateRoleAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Permission_Code_Does_Not_Exist()
    {
        Role role = CreateTenantRoleWithPermissions(Permission.Members.View.Name);
        _roleRepository.GetByIdAsync(role.Id, true, Arg.Any<CancellationToken>()).Returns(role);

        Result result = await _handler.Handle(
            new ReplaceRolePermissionsCommand(role.Id, ["NOT:EXISTS"]),
            default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.InvalidPermissionCode");
        await _roleRepository.DidNotReceive().RemovePermissionsAsync(Arg.Any<int>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Role_Does_Not_Exist()
    {
        _roleRepository.GetByIdAsync(999, true, Arg.Any<CancellationToken>()).Returns((Role?)null);

        Result result = await _handler.Handle(new ReplaceRolePermissionsCommand(999, [Permission.Members.View.Name]), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RoleErrors.NotFound);
    }

    private static Role CreateTenantRoleWithPermissions(params string[] permissionCodes)
    {
        Role role = Role.Create("OPS", TenantId);
        role.Id = 42;

        foreach (string code in permissionCodes)
        {
            role.Permissions.Add(Permission.CreateForRole(code, code, role.Id));
        }

        return role;
    }
}
