using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using Application.Abstractions.Infrastructure;
using Domain.Security;
using Domain.Tenants;
using Domain.Users;
using Domain.Gaming;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

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

        await EnsureDefaultTenantEntitlementsAsync(tenant.Id);
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

        // 中文註解：一次性補齊租戶綁定與角色綁定，避免同一個 User 在 Seeder 期間多次 SaveChanges 造成樂觀併發衝突。
        bool changed = false;

        if (!user.HasRole(role.Id))
        {
            user.AssignRole(role);
            changed = true;
        }

        if (!changed)
        {
            _logger.LogInformation("✅ DEF 租戶預設使用者綁定已完整: {Email}", DefaultTenantAdminEmail);
            return;
        }
        await _db.SaveChangesAsync();
    }

    private async Task<Role?> EnsureTenantAdminRoleAsync(Guid tenantId)
    {
        string normalizedRoleName = DefaultTenantAdminRoleName.Trim().ToUpperInvariant();
        Role? role = await _db.Set<Role>()
            .AsTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId
                && r.Name != null
                && r.Name == normalizedRoleName);

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
                // 注意：此處不要立刻 SaveChanges；統一由呼叫端在補齊 Role/Tenant 綁定後一次性存檔，
                // 可降低 Seeder 重入/多實例啟動時的樂觀併發衝突機率。
                existing.UpdateType(UserType.Tenant, tenant.Id);
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

    private async Task EnsureDefaultTenantEntitlementsAsync(Guid tenantId)
    {
        bool hasGameEntitlement = await _db.TenantGameEntitlements
            .AnyAsync(entitlement => entitlement.TenantId == tenantId);
        if (hasGameEntitlement)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;
        TenantGameEntitlement gameEntitlement = TenantGameEntitlement.Create(tenantId, GameCodes.Lottery539, now);
        TenantPlayEntitlement playEntitlement = TenantPlayEntitlement.Create(tenantId, GameCodes.Lottery539, PlayTypeCodes.Basic, now);

        await _db.TenantGameEntitlements.AddAsync(gameEntitlement);
        await _db.TenantPlayEntitlements.AddAsync(playEntitlement);

        ResourceNode? rootNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ParentId == null);

        if (rootNode is null)
        {
            rootNode = ResourceNode.Create("Tenant Root", "root", tenantId);
            await _db.ResourceNodes.AddAsync(rootNode);
        }

        string gameExternalKey = $"game:{GameCodes.Lottery539.Value}";
        ResourceNode? gameNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ExternalKey == gameExternalKey);
        if (gameNode is null)
        {
            gameNode = ResourceNode.Create(GameCodes.Lottery539.Value, gameExternalKey, tenantId, rootNode.Id);
            await _db.ResourceNodes.AddAsync(gameNode);
        }

        string playExternalKey = $"play:{GameCodes.Lottery539.Value}:{PlayTypeCodes.Basic.Value}";
        ResourceNode? playNode = await _db.ResourceNodes
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ExternalKey == playExternalKey);
        if (playNode is null)
        {
            playNode = ResourceNode.Create(PlayTypeCodes.Basic.Value, playExternalKey, tenantId, gameNode.Id);
            await _db.ResourceNodes.AddAsync(playNode);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("✅ 已建立 DEF 租戶預設遊戲啟用: {GameCode}", GameCodes.Lottery539.Value);
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
