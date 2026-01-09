using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using Application.Abstractions.Infrastructure;
using Domain.Security;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeders;

public sealed class DefaultTenantSeeder : IDataSeeder
{
    private const string DefaultTenantCode = "DEF";
    private const string DefaultTenantName = "Default Tenant";
    private const string DefaultTenantAdminRoleName = "ADMIN";
    private const string DefaultTenantAdminEmail = "rootdef@local.system";
    private const string DefaultTenantAdminPassword = "123456";

    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;
    private readonly ILogger<DefaultTenantSeeder> _logger;

    public DefaultTenantSeeder(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IConfiguration config,
        ILogger<DefaultTenantSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedDefaultDefTenantAsync();
    }

    private async Task SeedDefaultDefTenantAsync()
    {
        string code = DefaultTenantCode.Trim().ToUpperInvariant();
        string name = (_config["DefaultTenant:Name"] ?? DefaultTenantName).Trim();

        if (!IsValidTenantCode(code))
        {
            _logger.LogWarning("Default tenant code invalid: {Code}", code);
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Default tenant name is empty; skipping seeding.");
            return;
        }

        Tenant? tenant = await _db.Tenants.FirstOrDefaultAsync(existingTenant => existingTenant.Code == code);

        if (tenant is null)
        {
            tenant = Tenant.Create(Guid.NewGuid(), code, name);
            await _db.Tenants.AddAsync(tenant);
            await _db.SaveChangesAsync();

            _logger.LogInformation("✅ Default tenant created: {Code} - {Name}", code, name);
        }
        else
        {
            _logger.LogInformation("✅ Default tenant already exists: {Code}", code);
        }

        ResourceNode? rootNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenant.Id && node.ExternalKey == "root");

        if (rootNode is null)
        {
            rootNode = ResourceNode.Create(name, "root", tenant.Id);
            await _db.ResourceNodes.AddAsync(rootNode);
            await _db.SaveChangesAsync();
        }

        await EnsureSampleResourceTreeAsync(tenant.Id, rootNode.Id);

        await EnsureDefaultTenantAdminAsync(tenant);
    }

    private async Task EnsureSampleResourceTreeAsync(Guid tenantId, Guid rootNodeId)
    {
        ResourceNode? departmentNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ExternalKey == "department");

        if (departmentNode is null)
        {
            departmentNode = ResourceNode.Create("Default Department", "department", tenantId, rootNodeId);
            await _db.ResourceNodes.AddAsync(departmentNode);
            await _db.SaveChangesAsync();
        }

        ResourceNode? projectNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ExternalKey == "project");

        if (projectNode is null)
        {
            projectNode = ResourceNode.Create("Default Project", "project", tenantId, departmentNode.Id);
            await _db.ResourceNodes.AddAsync(projectNode);
            await _db.SaveChangesAsync();
        }
    }

    private async Task EnsureDefaultTenantAdminAsync(Tenant tenant)
    {
        if (!await SeederSchemaGuard.HasColumnAsync(_db, "Roles", "TenantId", _logger))
        {
            // 中文註解：若資料表尚未加入 Role.TenantId 欄位，先略過租戶角色種子流程。
            _logger.LogWarning("⚠️ Roles.TenantId 欄位尚未準備，略過 DEF 租戶管理者建立。");
            return;
        }

        Role? role = await EnsureTenantAdminRoleAsync(tenant.Id);
        if (role is null)
        {
            return;
        }

        User? user = await EnsureDefaultTenantUserAsync(tenant);
        if (user is null)
        {
            return;
        }

        if (!user.HasRole(role.Id))
        {
            user.AssignRole(role);
            await _db.SaveChangesAsync();
            _logger.LogInformation("✅ 已補齊 DEF 租戶管理者角色綁定: {Email}", DefaultTenantAdminEmail);
        }
    }

    private async Task<Role?> EnsureTenantAdminRoleAsync(Guid tenantId)
    {
        string normalizedRoleName = DefaultTenantAdminRoleName.Trim().ToUpperInvariant();
        Role? role = await _db.Set<Role>()
            .AsTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId
                && r.Name != null
                && r.Name.Trim().ToUpperInvariant() == normalizedRoleName);

        if (role is null)
        {
            role = Role.Create(DefaultTenantAdminRoleName, tenantId);
            await _db.Set<Role>().AddAsync(role);
            await _db.SaveChangesAsync();
            _logger.LogInformation("✅ 已建立 DEF 租戶管理者角色: {RoleName}", DefaultTenantAdminRoleName);
        }
        else
        {
            _logger.LogInformation("✅ DEF 租戶管理者角色已存在: {RoleName}", role.Name);
        }

        await EnsureRolePermissionsAsync(role, PermissionScope.Tenant);

        return role;
    }

    private async Task<User?> EnsureDefaultTenantUserAsync(Tenant tenant)
    {
        Email defaultEmail = Email.Create(DefaultTenantAdminEmail).Value;
        User? existing = await _db.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.Email == defaultEmail);

        if (existing is not null)
        {
            if (existing.Type != UserType.Tenant || existing.TenantId != tenant.Id)
            {
                // 中文註解：修正既有帳號型別與租戶綁定，避免錯誤分流。
                existing.UpdateType(UserType.Tenant, tenant.Id);
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("✅ DEF 租戶預設使用者已存在: {Email}", DefaultTenantAdminEmail);
            return existing;
        }

        User user = User.Create(
            defaultEmail,
            new Name("root-def"),
            _passwordHasher.Hash(DefaultTenantAdminPassword),
            false,
            UserType.Tenant,
            tenant.Id);

        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        _logger.LogInformation("✅ 已建立 DEF 租戶預設使用者: {Email}", DefaultTenantAdminEmail);

        return user;
    }

    private async Task EnsureRolePermissionsAsync(Role role, PermissionScope scope)
    {
        List<Permission> existingPermissions = await _db.Set<Permission>()
            .Where(permission => permission.RoleId == role.Id)
            .ToListAsync();

        HashSet<string> existingCodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (Permission permission in existingPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            existingCodes.Add(permission.Name.Trim().ToUpperInvariant());
        }

        List<string> expectedCodes = PermissionCatalog.AllPermissionCodes
            .Where(code =>
                PermissionCatalog.TryGetScope(code, out PermissionScope resolvedScope)
                && resolvedScope == scope)
            .Select(code => code.Trim().ToUpperInvariant())
            .ToList();

        List<Permission> toAdd = new List<Permission>();
        foreach (string code in expectedCodes)
        {
            if (existingCodes.Contains(code))
            {
                continue;
            }

            Permission permission = Permission.CreateForRole(code, code, role.Id);
            toAdd.Add(permission);
        }

        if (toAdd.Count == 0)
        {
            return;
        }

        await _db.Set<Permission>().AddRangeAsync(toAdd);
        await _db.SaveChangesAsync();
    }

    private static bool IsValidTenantCode(string tenantCode)
    {
        if (string.IsNullOrWhiteSpace(tenantCode))
        {
            return false;
        }

        if (tenantCode.Length != 3)
        {
            return false;
        }

        return tenantCode.All(char.IsLetterOrDigit);
    }
}
